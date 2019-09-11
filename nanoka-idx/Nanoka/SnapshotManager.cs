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

        readonly int _userId;
        readonly string _reason;

        public SnapshotManager(INanokaDatabase db, UserClaimSet claims)
        {
            _db = db;

            _userId = claims.Id;
            _reason = claims.Reason;
        }

        public Task UserCreated(User user, CancellationToken cancellationToken = default)
            => New(SnapshotType.System, SnapshotEvent.Creation, null as User, cancellationToken, user.Id);

        public Task BookUpdated(Book book, CancellationToken cancellationToken = default)
            => New(SnapshotType.User, SnapshotEvent.Modification, book, cancellationToken);

        public Task BookDeleted(Book book, CancellationToken cancellationToken = default)
            => New(SnapshotType.User, SnapshotEvent.Deletion, book, cancellationToken);

        async Task<Snapshot<T>> New<T>(SnapshotType type, SnapshotEvent @event, T value, CancellationToken cancellationToken, int? committer = null, string reason = null)
        {
            var snapshot = new Snapshot<T>
            {
                Time        = DateTime.UtcNow,
                CommitterId = committer ?? _userId,
                Type        = type,
                Event       = @event,
                Reason      = reason ?? _reason,
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