using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka
{
    public class SnapshotManager
    {
        readonly INanokaDatabase _db;
        readonly UserClaimSet _claims;
        readonly ILogger<SnapshotManager> _logger;

        public SnapshotManager(INanokaDatabase db, UserClaimSet claims, ILogger<SnapshotManager> logger)
        {
            _db     = db;
            _claims = claims;
            _logger = logger;
        }

        public async Task<Snapshot<T>> CreatedAsync<T>(SnapshotType type, T value, CancellationToken cancellationToken = default, string committer = null, string reason = null)
            where T : IHasId, IHasEntityType
        {
            var snapshot = new Snapshot<T>
            {
                Time        = DateTime.UtcNow,
                CommitterId = committer ?? _claims.Id,
                Type        = type,
                EntityType  = value.Type,
                EntityId    = value.Id,
                Event       = SnapshotEvent.Creation,
                Reason      = reason ?? _claims.Reason,
                Value       = value
            };

            await _db.UpdateSnapshotAsync(snapshot, cancellationToken);

            _logger.LogInformation("Created {0}", snapshot);

            return snapshot;
        }

        public async Task<Snapshot<T>> ModifiedAsync<T>(SnapshotType type, T value, CancellationToken cancellationToken = default, string committer = null, string reason = null)
            where T : IHasId, IHasEntityType
        {
            var snapshot = new Snapshot<T>
            {
                Time        = DateTime.UtcNow,
                CommitterId = committer ?? _claims.Id,
                Type        = type,
                EntityType  = value.Type,
                EntityId    = value.Id,
                Event       = SnapshotEvent.Modification,
                Reason      = reason ?? _claims.Reason,
                Value       = value
            };

            await _db.UpdateSnapshotAsync(snapshot, cancellationToken);

            _logger.LogInformation("Modified {0}", snapshot);

            return snapshot;
        }

        public async Task<Snapshot<T>> DeletedAsync<T>(SnapshotType type, T value, CancellationToken cancellationToken = default, string committer = null, string reason = null)
            where T : IHasId, IHasEntityType
        {
            var snapshot = new Snapshot<T>
            {
                Time        = DateTime.UtcNow,
                CommitterId = committer ?? _claims.Id,
                Type        = type,
                EntityType  = value.Type,
                EntityId    = value.Id,
                Event       = SnapshotEvent.Deletion,
                Reason      = reason ?? _claims.Reason
            };

            await _db.UpdateSnapshotAsync(snapshot, cancellationToken);

            _logger.LogInformation("Deleted {0}", snapshot);

            return snapshot;
        }

        public async Task<Snapshot<T>> RevertedAsync<T>(SnapshotType type, T value, Snapshot<T> previous, CancellationToken cancellationToken = default, string committer = null, string reason = null)
            where T : IHasId, IHasEntityType
        {
            var snapshot = new Snapshot<T>
            {
                Time        = DateTime.UtcNow,
                RollbackId  = previous.Id,
                CommitterId = committer ?? _claims.Id,
                Type        = type,
                EntityType  = value?.Type ?? previous.EntityType,
                EntityId    = value?.Id ?? previous.EntityId,
                Event       = SnapshotEvent.Rollback,
                Reason      = reason ?? _claims.Reason,
                Value       = value
            };

            await _db.UpdateSnapshotAsync(snapshot, cancellationToken);

            _logger.LogInformation("Reverted {0}", snapshot);

            return snapshot;
        }

        public async Task<Snapshot<T>[]> GetAsync<T>(string entityId, CancellationToken cancellationToken = default)
        {
            var snapshots = await _db.GetSnapshotsAsync<T>(entityId, cancellationToken);

            if (snapshots.Length == 0)
                throw Result.NotFound<T>(entityId).Exception;

            return snapshots;
        }

        public async Task<Snapshot<T>> GetAsync<T>(string id, string entityId, CancellationToken cancellationToken = default)
        {
            var snapshot = await _db.GetSnapshotAsync<T>(id, entityId, cancellationToken);

            if (snapshot == null)
                throw Result.NotFound<Snapshot<T>>(entityId, id).Exception;

            return snapshot;
        }
    }
}