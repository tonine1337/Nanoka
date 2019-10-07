using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Database;
using Nanoka.Models;
using Nanoka.Models.Requests;
using Nanoka.Storage;

namespace Nanoka.Controllers
{
    [ApiController]
    [Route("books")]
    [Authorize]
    public class BookController : ControllerBase
    {
        readonly IBookRepository _books;
        readonly ILocker _locker;
        readonly IStorage _storage;
        readonly IMapper _mapper;
        readonly UploadManager _uploads;
        readonly SnapshotManager _snapshots;
        readonly ImageProcessor _image;
        readonly VoteManager _votes;

        public BookController(IBookRepository books, ILocker locker, IStorage storage, IMapper mapper,
                              UploadManager uploads, SnapshotManager snapshots, ImageProcessor image, VoteManager votes)
        {
            _books     = books;
            _locker    = locker;
            _storage   = storage;
            _mapper    = mapper;
            _uploads   = uploads;
            _snapshots = snapshots;
            _image     = image;
            _votes     = votes;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetAsync(string id)
        {
            var book = await _books.GetAsync(id);

            if (book == null)
                return ResultUtilities.NotFound<Book>(id);

            return book;
        }

        [HttpPut("{id}")]
        [UserClaims(unrestricted: true)]
        public async Task<ActionResult<Book>> UpdateAsync(string id, BookBase model)
        {
            using (await _locker.EnterAsync(id))
            {
                var book = await _books.GetAsync(id);

                if (book == null)
                    return ResultUtilities.NotFound<Book>(id);

                _mapper.Map(model, book);

                await _books.UpdateAsync(book);
                await _snapshots.ModifiedAsync(book);

                return book;
            }
        }

        [HttpDelete("{id}")]
        [UserClaims(unrestricted: true, reputation: 100, reason: true)]
        [VerifyHuman]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            using (await _locker.EnterAsync(id))
            {
                var book = await _books.GetAsync(id);

                if (book == null)
                    return ResultUtilities.NotFound<Book>(id);

                await _books.DeleteAsync(book);
                await _snapshots.DeletedAsync(book);
                await _storage.DeleteAsync(GetFileNames(book));
            }

            return Ok();
        }

        static string[] GetFileNames(Book book) => book.Contents?.ToArrayMany(c => GetFileNames(book, c)) ?? new string[0];

        static string[] GetFileNames(IHasId book, BookContent content)
        {
            var names = new string[content.PageCount];

            for (var i = 0; i < names.Length; i++)
                names[i] = $"{book.Id}/{content.Id}/{i + 1}";

            return names;
        }

        [HttpGet("{id}/snapshots")]
        public async Task<Snapshot<Book>[]> GetSnapshotsAsync(string id)
            => await _snapshots.GetAsync<Book>(id);

        [HttpPost("{id}/snapshots/revert")]
        [UserClaims(unrestricted: true, reason: true)]
        public async Task<ActionResult<Book>> RevertAsync(string id, RevertEntityRequest request)
        {
            using (await _locker.EnterAsync(id))
            {
                var snapshot = await _snapshots.GetAsync<Book>(id, request.SnapshotId);

                if (snapshot == null)
                    return ResultUtilities.NotFound<Snapshot>(id, request.SnapshotId);

                var book = await _books.GetAsync(id);

                if (book != null && snapshot.Value == null)
                {
                    await _books.DeleteAsync(book);
                    await _storage.DeleteAsync(GetFileNames(book));

                    book = null;
                }

                else if (snapshot.Value != null)
                {
                    book = snapshot.Value;

                    await _books.UpdateAsync(book);

                    if (_storage is ISupportsUndelete supportsUndelete)
                        await supportsUndelete.UndeleteAsync(GetFileNames(book));
                }

                await _snapshots.RevertedAsync(snapshot);

                return book;
            }
        }

        [HttpPut("{id}/vote")]
        public async Task<ActionResult<Vote>> SetVoteAsync(string id, VoteBase model)
        {
            using (await _locker.EnterAsync(id))
            {
                var book = await _books.GetAsync(id);

                if (book == null)
                    return ResultUtilities.NotFound<Book>(id);

                var vote = await _votes.SetAsync(book, model.Type);

                // score is updated
                await _books.UpdateAsync(book);

                return vote;
            }
        }

        [HttpDelete("{id}/vote")]
        public async Task<ActionResult> UnsetVoteAsync(string id)
        {
            using (await _locker.EnterAsync(id))
            {
                var book = await _books.GetAsync(id);

                if (book == null)
                    return ResultUtilities.NotFound<Book>(id);

                await _votes.SetAsync(book, null);

                // score is updated
                await _books.UpdateAsync(book);

                return Ok();
            }
        }

        [HttpGet("{id}/contents/{contentId}")]
        public async Task<ActionResult<BookContent>> GetContentAsync(string id, string contentId)
        {
            var book = await _books.GetAsync(id);

            if (book == null)
                return ResultUtilities.NotFound<Book>(id);

            var content = book.Contents.FirstOrDefault(c => c.Id == contentId);

            if (content == null)
                return ResultUtilities.NotFound<BookContent>(id, contentId);

            return content;
        }

        [HttpPut("{id}/contents/{contentId}")]
        public async Task<ActionResult<BookContent>> UpdateContentAsync(string id, string contentId, BookContentBase model)
        {
            using (await _locker.EnterAsync(id))
            {
                var book = await _books.GetAsync(id);

                if (book == null)
                    return ResultUtilities.NotFound<Book>(id);

                var content = book.Contents.FirstOrDefault(c => c.Id == contentId);

                if (content == null)
                    return ResultUtilities.NotFound<BookContent>(id, contentId);

                _mapper.Map(model, content);

                await _books.UpdateAsync(book);
                await _snapshots.ModifiedAsync(book);

                return content;
            }
        }

        [HttpDelete("{id}/contents/{contentId}")]
        [UserClaims(unrestricted: true, reputation: 100, reason: true)]
        [VerifyHuman]
        public async Task<ActionResult> DeleteContentAsync(string id, string contentId)
        {
            using (await _locker.EnterAsync(id))
            {
                var book = await _books.GetAsync(id);

                if (book == null)
                    return ResultUtilities.NotFound<Book>(id);

                var content = book.Contents.FirstOrDefault(c => c.Id == contentId);

                if (content == null)
                    return ResultUtilities.NotFound<BookContent>(id, contentId);

                if (book.Contents.Length == 1)
                {
                    // delete the entire book
                    await _books.DeleteAsync(book);
                    await _snapshots.DeletedAsync(book);
                }
                else
                {
                    // remove the content
                    book.Contents = book.Contents.Where(c => c != content).ToArray();

                    await _books.UpdateAsync(book);
                    await _snapshots.ModifiedAsync(book);
                }

                await _storage.DeleteAsync(GetFileNames(book, content));

                return Ok();
            }
        }

        [HttpGet("{id}/contents/{contentId}/images/{index}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetImageAsync(string id, string contentId, int index)
        {
            var file = await _storage.ReadAsync($"{id}/{contentId}/{index}");

            if (file == null)
                return ResultUtilities.NotFound<StorageFile>(id, contentId, index);

            var stream    = file.Stream;
            var mediaType = file.MediaType;

            return new FileStreamResult(stream, mediaType);
        }

        [HttpPost("search")]
        public async Task<SearchResult<Book>> SearchAsync(BookQuery query)
            => await _books.SearchAsync(query);

        sealed class BookUpload
        {
            public string BookId;

            public BookBase Book;
            public BookContentBase Content;
        }

        [HttpPost("uploads")]
        [UserClaims(unrestricted: true)]
        public async Task<ActionResult<UploadState>> CreateUploadAsync(CreateNewBookRequest request)
        {
            // creating an entirely new book
            if (request.Book != null)
                return _uploads.CreateTask(new BookUpload
                {
                    Book    = request.Book,
                    Content = request.Content
                });

            // adding contents to an existing book
            if (request.BookId != null)
            {
                var book = await _books.GetAsync(request.BookId);

                if (book == null)
                    return ResultUtilities.NotFound<Book>(request.BookId);

                return _uploads.CreateTask(new BookUpload
                {
                    BookId  = book.Id,
                    Content = request.Content
                });
            }

            throw new ArgumentException("Invalid upload state.");
        }

        [HttpGet("uploads/{id}")]
        public UploadState GetUpload(string id)
            => _uploads.GetTask<BookUpload>(id);

        [HttpPost("uploads/{id}/files")]
        public async Task<UploadState> UploadFileAsync(string id, [FromForm(Name = "file")] IFormFile file)
        {
            var task = _uploads.GetTask<BookUpload>(id);

            var (stream, mediaType) = await _image.LoadAsync(file);

            using (stream)
                await task.AddFileAsync(null, stream, mediaType);

            return task;
        }

        [HttpDelete("uploads/{id}")]
        public async Task<ActionResult<Book>> DeleteUploadAsync(string id, [FromQuery] bool commit)
        {
            using (var task = _uploads.RemoveTask<BookUpload>(id))
            {
                if (!commit)
                    return Ok();

                if (task.FileCount == 0)
                    return BadRequest("No files were uploaded to be committed.");

                if (task.Data.Book != null)
                {
                    var book    = _mapper.Map<Book>(task.Data.Book);
                    var content = _mapper.Map<BookContent>(task.Data.Content);

                    content.Id        = Snowflake.New;
                    content.PageCount = task.FileCount;

                    book.Contents = new[] { content };

                    await _books.UpdateAsync(book);
                    await _snapshots.CreatedAsync(book);

                    await UploadContentAsync(book, content, task);

                    return book;
                }

                if (task.Data.BookId != null)
                    using (await _locker.EnterAsync(id))
                    {
                        var book = await _books.GetAsync(id);

                        if (book == null)
                            return ResultUtilities.NotFound<Book>(id);

                        var content = _mapper.Map<BookContent>(task.Data.Content);

                        content.Id        = Snowflake.New;
                        content.PageCount = task.FileCount;

                        book.Contents = book.Contents.Append(content).ToArray();

                        await _books.UpdateAsync(book);
                        await _snapshots.ModifiedAsync(book);

                        await UploadContentAsync(book, content, task);

                        return book;
                    }

                throw new InvalidOperationException("Invalid upload state.");
            }
        }

        async Task UploadContentAsync(IHasId book, BookContent content, UploadTask task)
        {
            var names = GetFileNames(book, content);

            var array = task.EnumerateFiles().ToArray();

            for (var i = 0; i < array.Length; i++)
            {
                var (_, s, m) = array[i];

                await _storage.WriteAsync(new StorageFile
                {
                    Name      = names[i],
                    Stream    = s,
                    MediaType = m
                });
            }
        }
    }
}