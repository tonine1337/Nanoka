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

        public Task<Snapshot<T>> Creation<T>(SnapshotType type, T value, CancellationToken cancellationToken = default, int? committer = null, string reason = null)
            where T : IHasId, IHasEntityType
        {
            var snapshot = new Snapshot<T>
            {
                CommitterId = committer ?? _claims.Id,
                Type        = type,
                EntityType  = value.Type,
                EntityId    = value.Id,
                Event       = SnapshotEvent.Creation,
                Reason      = reason ?? _claims.Reason
            };

            return AddAsync(snapshot, cancellationToken);
        }

        public Task<Snapshot<T>> Modification<T>(SnapshotType type, T value, CancellationToken cancellationToken = default, int? committer = null, string reason = null)
            where T : IHasId, IHasEntityType
        {
            var snapshot = new Snapshot<T>
            {
                CommitterId = committer ?? _claims.Id,
                Type        = type,
                EntityType  = value.Type,
                EntityId    = value.Id,
                Event       = SnapshotEvent.Modification,
                Reason      = reason ?? _claims.Reason,
                Value       = value
            };

            return AddAsync(snapshot, cancellationToken);
        }

        public Task<Snapshot<T>> Deletion<T>(SnapshotType type, T value, CancellationToken cancellationToken = default, int? committer = null, string reason = null)
            where T : IHasId, IHasEntityType
        {
            var snapshot = new Snapshot<T>
            {
                CommitterId = committer ?? _claims.Id,
                Type        = type,
                EntityType  = value.Type,
                EntityId    = value.Id,
                Event       = SnapshotEvent.Deletion,
                Reason      = reason ?? _claims.Reason,
                Value       = value
            };

            return AddAsync(snapshot, cancellationToken);
        }

        public Task<Snapshot<T>> Rollback<T>(SnapshotType type, T value, Snapshot<T> previous, CancellationToken cancellationToken = default, int? committer = null, string reason = null)
            where T : IHasId, IHasEntityType
        {
            var snapshot = new Snapshot<T>
            {
                RollbackId  = previous.Id,
                CommitterId = committer ?? _claims.Id,
                Type        = type,
                EntityType  = value.Type,
                EntityId    = value.Id,
                Event       = SnapshotEvent.Rollback,
                Reason      = reason ?? _claims.Reason,
                Value       = value
            };

            return AddAsync(snapshot, cancellationToken);
        }

        public async Task<Snapshot<T>> AddAsync<T>(Snapshot<T> snapshot, CancellationToken cancellationToken = default)
        {
            snapshot.Time = DateTime.UtcNow;

            // require reason for configured event types
            if (snapshot.Type != SnapshotType.System && _options.RequireReasonForEvents.Contains(snapshot.Event) && string.IsNullOrWhiteSpace(snapshot.Reason))
                throw Result.BadRequest($"{snapshot.Event} of {snapshot.EntityType} '{snapshot.EntityId}': reason must be specified for this action.").Exception;

            await _db.UpdateSnapshotAsync(snapshot, cancellationToken);

            return snapshot;
        }
    }
}