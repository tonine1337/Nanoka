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

        public async Task<Snapshot<T>> AddAsync<T>(SnapshotType type, SnapshotEvent @event, T value, CancellationToken cancellationToken = default, int? committer = null, string reason = null)
            where T : IHasId, ISupportSnapshot
        {
            var snapshot = new Snapshot<T>
            {
                Time        = DateTime.UtcNow,
                CommitterId = committer ?? _userId,
                Type        = type,
                Entity      = value.EntityType,
                EntityId    = value.Id,
                Event       = @event,
                Reason      = reason ?? _reason,
                Value       = value
            };

            await _db.UpdateSnapshotAsync(snapshot, cancellationToken);

            return snapshot;
        }
    }
}