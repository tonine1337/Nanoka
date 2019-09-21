using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Nanoka.Database;
using Nanoka.Models;
using Nanoka.Storage;

namespace Nanoka
{
    public class BookManager
    {
        readonly INanokaDatabase _db;
        readonly ILocker _locker;
        readonly IMapper _mapper;
        readonly SnapshotManager _snapshot;
        readonly VoteManager _vote;
        readonly IStorage _storage;

        public BookManager(INanokaDatabase db, ILocker locker, IMapper mapper, SnapshotManager snapshot, VoteManager vote,
                           IStorage storage)
        {
            _db       = db;
            _locker   = locker;
            _mapper   = mapper;
            _snapshot = snapshot;
            _vote     = vote;
            _storage  = storage;
        }

        public async Task<Book> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            var book = await _db.GetBookAsync(id, cancellationToken);

            if (book == null)
                throw Result.NotFound<Book>(id).Exception;

            return book;
        }

        public async Task<(Book book, BookContent content)> GetContentAsync(string id, string contentId, CancellationToken cancellationToken = default)
        {
            var book    = await GetAsync(id, cancellationToken);
            var content = book.Contents.FirstOrDefault(c => c.Id == contentId);

            if (content == null)
                throw Result.NotFound<BookContent>(id, contentId).Exception;

            return (book, content);
        }

        public Task<Snapshot<Book>[]> GetSnapshotsAsync(string id, CancellationToken cancellationToken = default)
            => _snapshot.GetAsync<Book>(id, cancellationToken);

        public async Task<Book> RevertAsync(string id, string snapshotId, CancellationToken cancellationToken = default)
        {
            using (await _locker.EnterAsync(id, cancellationToken))
            {
                var snapshot = await _snapshot.GetAsync<Book>(snapshotId, id, cancellationToken);
                var book     = await _db.GetBookAsync(id, cancellationToken);

                if (book != null && snapshot.Value == null)
                {
                    await _db.DeleteBookAsync(book, cancellationToken);
                    await _storage.DeleteAsync(GetBookFiles(book), cancellationToken);

                    book = null;
                }
                else if (snapshot.Value != null)
                {
                    book = snapshot.Value;

                    await _db.UpdateBookAsync(book, cancellationToken);

                    if (_storage is ISupportsUndelete supportsUndelete)
                        await supportsUndelete.UndeleteAsync(GetBookFiles(book), cancellationToken);
                }

                await _snapshot.RevertedAsync(snapshot, cancellationToken);

                return book;
            }
        }

        public async Task<Book> UpdateAsync(string id, BookBase model, CancellationToken cancellationToken = default)
        {
            using (await _locker.EnterAsync(id, cancellationToken))
            {
                var book = await GetAsync(id, cancellationToken);

                _mapper.Map(model, book);

                await _db.UpdateBookAsync(book, cancellationToken);
                await _snapshot.ModifiedAsync(book, cancellationToken);

                return book;
            }
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            using (await _locker.EnterAsync(id, cancellationToken))
            {
                var book = await GetAsync(id, cancellationToken);

                await _db.DeleteBookAsync(book, cancellationToken);
                await _snapshot.DeletedAsync(book, cancellationToken);
                await _storage.DeleteAsync(GetBookFiles(book), cancellationToken);
            }
        }

        static string[] GetBookFiles(Book book) => book.Contents?.ToArrayMany(c => GetBookFiles(book, c)) ?? new string[0];

        static string[] GetBookFiles(Book book, BookContent content)
        {
            var names = new string[content.PageCount];

            for (var i = 0; i < names.Length; i++)
                names[i] = $"{book.Id}/{content.Id}/{i + 1}";

            return names;
        }

        public async Task<Vote> VoteAsync(string id, VoteType? type, CancellationToken cancellationToken = default)
        {
            using (await _locker.EnterAsync(id, cancellationToken))
            {
                var book = await GetAsync(id, cancellationToken);
                var vote = await _vote.SetAsync(book, type, cancellationToken);

                if (vote == null)
                    return null;

                // score is updated
                await _db.UpdateBookAsync(book, cancellationToken);

                return vote;
            }
        }

        public async Task<Book> CreateAsync(BookBase bookModel, BookContentBase contentModel, UploadTask uploadTask, CancellationToken cancellationToken = default)
        {
            var book    = _mapper.Map<Book>(bookModel);
            var content = _mapper.Map<BookContent>(contentModel);

            content.Id        = Snowflake.New;
            content.PageCount = uploadTask.FileCount;

            book.Contents = new[] { content };

            await _db.UpdateBookAsync(book, cancellationToken);
            await _snapshot.CreatedAsync(book, cancellationToken);

            await UploadContentAsync(book, content, uploadTask, cancellationToken);

            return book;
        }

        public async Task<(Book, BookContent)> AddContentAsync(string id, BookContentBase model, UploadTask uploadTask, CancellationToken cancellationToken = default)
        {
            using (await _locker.EnterAsync(id, cancellationToken))
            {
                var book = await GetAsync(id, cancellationToken);

                var content = _mapper.Map<BookContent>(model);

                content.Id        = Snowflake.New;
                content.PageCount = uploadTask.FileCount;

                book.Contents = book.Contents.Append(content).ToArray();

                await _db.UpdateBookAsync(book, cancellationToken);
                await _snapshot.ModifiedAsync(book, cancellationToken);

                await UploadContentAsync(book, content, uploadTask, cancellationToken);

                return (book, content);
            }
        }

        async Task UploadContentAsync(Book book, BookContent content, UploadTask uploadTask, CancellationToken cancellationToken = default)
        {
            var files = GetBookFiles(book, content).Zip(uploadTask.EnumerateFiles(), (n, f) => new StorageFile(n, f.stream, f.mediaType));

            foreach (var file in files)
            {
                using (file)
                    await _storage.WriteAsync(file, cancellationToken);
            }
        }

        public async Task<BookContent> UpdateContentAsync(string id, string contentId, BookContentBase model, CancellationToken cancellationToken = default)
        {
            using (await _locker.EnterAsync(id, cancellationToken))
            {
                var (book, content) = await GetContentAsync(id, contentId, cancellationToken);

                _mapper.Map(model, content);

                await _db.UpdateBookAsync(book, cancellationToken);
                await _snapshot.ModifiedAsync(book, cancellationToken);

                return content;
            }
        }

        public async Task RemoveContentAsync(string id, string contentId, CancellationToken cancellationToken = default)
        {
            using (await _locker.EnterAsync(id, cancellationToken))
            {
                var (book, content) = await GetContentAsync(id, contentId, cancellationToken);

                if (book.Contents.Length == 1)
                {
                    // delete the entire book
                    await _db.DeleteBookAsync(book, cancellationToken);
                    await _snapshot.DeletedAsync(book, cancellationToken);
                }
                else
                {
                    // remove the content
                    book.Contents = book.Contents.Where(c => c != content).ToArray();

                    await _db.UpdateBookAsync(book, cancellationToken);
                    await _snapshot.ModifiedAsync(book, cancellationToken);
                }

                await _storage.DeleteAsync(GetBookFiles(book, content), cancellationToken);
            }
        }

        public Task<SearchResult<Book>> SearchAsync(BookQuery query, CancellationToken cancellationToken = default)
            => _db.SearchBooksAsync(query, cancellationToken);
    }
}