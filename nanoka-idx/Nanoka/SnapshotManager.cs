using System;
using System.Threading;
using System.Threading.Tasks;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka
{
    public class SnapshotManager
    {
        readonly INanokaDatabase _db;

        public SnapshotManager(INanokaDatabase db)
        {
            _db = db;
        }

        public Task UserCreated(User user, CancellationToken cancellationToken = default)
            => New(user.Id, SnapshotType.System, SnapshotEvent.Creation, null as User, null, cancellationToken);

        public Task BookUpdated(Book book, int userId, string reason, CancellationToken cancellationToken = default)
            => New(userId, SnapshotType.User, SnapshotEvent.Modification, book, reason, cancellationToken);

        public Task BookDeleted(Book book, int userId, string reason, CancellationToken cancellationToken = default)
            => New(userId, SnapshotType.User, SnapshotEvent.Deletion, book, reason, cancellationToken);

        async Task<Snapshot<T>> New<T>(int committer, SnapshotType type, SnapshotEvent @event, T value, string reason, CancellationToken cancellationToken)
        {
            var snapshot = new Snapshot<T>
            {
                Time        = DateTime.UtcNow,
                CommitterId = committer,
                Type        = type,
                Event       = @event,
                Reason      = reason,
                Value       = value
            };

            switch (value)
            {
                case User user:
                    snapshot.Entity   = SnapshotEntity.User;
                    snapshot.EntityId = user.Id;
                    break;

                default:
                    snapshot.Entity = SnapshotEntity.Unknown;
                    break;
            }

            snapshot.Id = await _db.AddSnapshotAsync(snapshot, cancellationToken);

            return snapshot;
        }
    }
}