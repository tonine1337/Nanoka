using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka
{
    public class SnapshotManager
    {
        readonly NanokaOptions _options;
        readonly INanokaDatabase _db;
        readonly UserClaimSet _claims;

        public SnapshotManager(IOptions<NanokaOptions> options, INanokaDatabase db, UserClaimSet claims)
        {
            _options = options.Value;
            _db      = db;
            _claims  = claims;
        }

        public Task<Snapshot<T>> CreatedAsync<T>(SnapshotType type, T value, CancellationToken cancellationToken = default, int? committer = null, string reason = null)
            where T : IHasId, IHasEntityType
            => SnapshotInternal(type, SnapshotEvent.Creation, value, cancellationToken, null, committer, reason);

        public Task<Snapshot<T>> ModifiedAsync<T>(SnapshotType type, T value, CancellationToken cancellationToken = default, int? committer = null, string reason = null)
            where T : IHasId, IHasEntityType
            => SnapshotInternal(type, SnapshotEvent.Modification, value, cancellationToken, null, committer, reason);

        public Task<Snapshot<T>> DeletedAsync<T>(SnapshotType type, T value, CancellationToken cancellationToken = default, int? committer = null, string reason = null)
            where T : IHasId, IHasEntityType
            => SnapshotInternal(type, SnapshotEvent.Deletion, value, cancellationToken, null, committer, reason);

        public Task<Snapshot<T>> RolledBackAsync<T>(SnapshotType type, T value, Snapshot<T> previous, CancellationToken cancellationToken = default, int? committer = null, string reason = null)
            where T : IHasId, IHasEntityType
            => SnapshotInternal(type, SnapshotEvent.Rollback, value, cancellationToken, previous.Id, committer, reason);

        async Task<Snapshot<T>> SnapshotInternal<T>(SnapshotType type, SnapshotEvent @event, T value, CancellationToken cancellationToken = default,
                                                    int? rollbackId = null, int? committer = null, string reason = null)
            where T : IHasId, IHasEntityType
        {
            var snapshot = new Snapshot<T>
            {
                Time        = DateTime.UtcNow,
                RollbackId  = rollbackId,
                CommitterId = committer ?? _claims.Id,
                Type        = type,
                EntityType  = value.Type,
                EntityId    = value.Id,
                Event       = @event,
                Reason      = reason ?? _claims.Reason,
                Value       = value
            };

            if (@event == SnapshotEvent.Deletion)
                snapshot.Value = default;

            // require reason for configured event types
            if (snapshot.Type != SnapshotType.System && _options.RequireReasonForEvents.Contains(snapshot.Event) && string.IsNullOrWhiteSpace(snapshot.Reason))
                throw Result.BadRequest($"{snapshot.Event} of {snapshot.EntityType} {snapshot.EntityId}: reason must be specified for this action.").Exception;

            await _db.UpdateSnapshotAsync(snapshot, cancellationToken);

            return snapshot;
        }

        public async Task<Snapshot<T>[]> GetAsync<T>(int entityId, CancellationToken cancellationToken = default)
        {
            var snapshots = await _db.GetSnapshotsAsync<T>(entityId, cancellationToken);

            if (snapshots.Length == 0)
                throw Result.NotFound<T>(entityId).Exception;

            return snapshots;
        }

        public async Task<Snapshot<T>> GetAsync<T>(int id, int entityId, CancellationToken cancellationToken = default)
        {
            var snapshot = await _db.GetSnapshotAsync<T>(id, entityId, cancellationToken);

            if (snapshot == null)
                throw Result.NotFound<Snapshot<T>>(entityId, id).Exception;

            return snapshot;
        }
    }
}