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

        public Task<Snapshot<Book>[]> GetSnapshotsAsync(int id, CancellationToken cancellationToken = default)
            => _db.GetSnapshotsAsync<Book>(id, cancellationToken);

        public async Task<Book> RevertAsync(int id, int snapshotId, CancellationToken cancellationToken = default)
        {
            using (await _locker.EnterAsync(id, cancellationToken))
            {
                var snapshot = await _db.GetSnapshotAsync<Book>(snapshotId, id, cancellationToken);

                if (snapshot == null)
                    throw new SnapshotManagerException($"Snapshot '{snapshotId}' of book '{id}' does not exist.");

                // create snapshot of current value
                await _snapshot.AddAsync(SnapshotType.User, SnapshotEvent.Rollback, await _db.GetBookAsync(id, cancellationToken), cancellationToken);

                // update current value to loaded snapshot value
                await _db.UpdateBookAsync(snapshot.Value, cancellationToken);

                return snapshot.Value;
            }
        }

        public async Task<Book> UpdateAsync(int id, BookBase model, CancellationToken cancellationToken = default)
        {
            using (await _locker.EnterAsync(id, cancellationToken))
            {
                var book = await _db.GetBookAsync(id, cancellationToken);

                if (book == null)
                    throw new BookManagerException($"Book '{id}' does not exist.");

                await _snapshot.AddAsync(SnapshotType.User, SnapshotEvent.Modification, book, cancellationToken);

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

                await _snapshot.AddAsync(SnapshotType.User, SnapshotEvent.Deletion, book, cancellationToken);

                await _db.DeleteBookAsync(book, cancellationToken);
            }
        }
    }
}