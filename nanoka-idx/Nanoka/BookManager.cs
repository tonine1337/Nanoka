using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka
{
    public class BookManager
    {
        readonly INanokaDatabase _db;
        readonly ILocker _locker;
        readonly SnapshotManager _snapshot;
        readonly IMapper _mapper;

        public BookManager(INanokaDatabase db, NamedLocker locker, SnapshotManager snapshot, IMapper mapper)
        {
            _db       = db;
            _locker   = locker.Get<BookManager>();
            _snapshot = snapshot;
            _mapper   = mapper;
        }

        public Task<Book> GetAsync(int id, CancellationToken cancellationToken = default)
            => _db.GetBookAsync(id, cancellationToken);

        public async Task<Book> UpdateAsync(int id, BookBase model, CancellationToken cancellationToken = default)
        {
            using (await _locker.EnterAsync(id, cancellationToken))
            {
                var book = await _db.GetBookAsync(id, cancellationToken);

                if (book == null)
                    throw new BookManagerException($"Book '{id}' does not exist.");

                await _snapshot.BookUpdated(book, cancellationToken);

                _mapper.Map(model, book);

                await _db.UpdateBookAsync(book, cancellationToken);

                return book;
            }
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            using (await _locker.EnterAsync(id, cancellationToken))
            {
                var book = await _db.GetBookAsync(id, cancellationToken);

                if (book == null)
                    throw new BookManagerException($"Book '{id}' does not exist.");

                await _db.DeleteBookAsync(id, cancellationToken);

                await _snapshot.BookDeleted(book, cancellationToken);
            }
        }
    }
}