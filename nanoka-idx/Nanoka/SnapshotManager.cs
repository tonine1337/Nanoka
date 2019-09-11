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

        public Task UserCreatedAsync(User user, CancellationToken cancellationToken = default)
            => New<User>(user.Id, SnapshotType.System, SnapshotEvent.Creation, null, null, cancellationToken);

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