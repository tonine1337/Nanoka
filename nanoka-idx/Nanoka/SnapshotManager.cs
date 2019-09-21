using System;
using System.Collections.Generic;
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
        readonly IUserClaims _claims;
        readonly ILogger<SnapshotManager> _logger;

        public SnapshotManager(INanokaDatabase db, IUserClaims claims, ILogger<SnapshotManager> logger)
        {
            _db     = db;
            _claims = claims;
            _logger = logger;
        }

        SnapshotType SuitableType => _claims.HasPermissions(UserPermissions.Moderator) ? SnapshotType.Moderator : SnapshotType.User;

        public async Task<Snapshot<T>> CreatedAsync<T>(T value, CancellationToken cancellationToken = default, SnapshotType? type = null, string committer = null, string reason = null)
            where T : IHasId, IHasEntityType
        {
            var snapshot = new Snapshot<T>
            {
                Time        = DateTime.UtcNow,
                CommitterId = committer ?? _claims.Id,
                Type        = type ?? SuitableType,
                EntityType  = value.Type,
                EntityId    = value.Id,
                Event       = SnapshotEvent.Creation,
                Reason      = reason ?? _claims.GetReason(),
                Value       = value
            };

            await _db.UpdateSnapshotAsync(snapshot, cancellationToken);

            _logger.LogInformation("Created {0}", snapshot);

            return snapshot;
        }

        public async Task<Snapshot<T>> ModifiedAsync<T>(T value, CancellationToken cancellationToken = default, SnapshotType? type = null, string committer = null, string reason = null)
            where T : IHasId, IHasEntityType
        {
            var snapshot = new Snapshot<T>
            {
                Time        = DateTime.UtcNow,
                CommitterId = committer ?? _claims.Id,
                Type        = type ?? SuitableType,
                EntityType  = value.Type,
                EntityId    = value.Id,
                Event       = SnapshotEvent.Modification,
                Reason      = reason ?? _claims.GetReason(),
                Value       = value
            };

            await _db.UpdateSnapshotAsync(snapshot, cancellationToken);

            _logger.LogInformation("Modified {0}", snapshot);

            return snapshot;
        }

        public async Task<Snapshot<T>> DeletedAsync<T>(T value, CancellationToken cancellationToken = default, SnapshotType? type = null, string committer = null, string reason = null)
            where T : IHasId, IHasEntityType
        {
            var snapshot = new Snapshot<T>
            {
                Time        = DateTime.UtcNow,
                CommitterId = committer ?? _claims.Id,
                Type        = type ?? SuitableType,
                EntityType  = value.Type,
                EntityId    = value.Id,
                Event       = SnapshotEvent.Deletion,
                Reason      = reason ?? _claims.GetReason()
            };

            await _db.UpdateSnapshotAsync(snapshot, cancellationToken);

            _logger.LogInformation("Deleted {0}", snapshot);

            return snapshot;
        }

        public async Task<Snapshot<T>> RevertedAsync<T>(Snapshot<T> targetRollback, CancellationToken cancellationToken = default, SnapshotType? type = null, string committer = null, string reason = null)
            where T : IHasId, IHasEntityType
        {
            var snapshot = new Snapshot<T>
            {
                Time        = DateTime.UtcNow,
                RollbackId  = targetRollback.Id,
                CommitterId = committer ?? _claims.Id,
                Type        = type ?? SuitableType,
                EntityType  = targetRollback.EntityType,
                EntityId    = targetRollback.EntityId,
                Event       = SnapshotEvent.Rollback,
                Reason      = reason ?? _claims.GetReason(),
                Value       = targetRollback.Value
            };

            await _db.UpdateSnapshotAsync(snapshot, cancellationToken);

            _logger.LogInformation("Reverted {0}", snapshot);

            return snapshot;
        }

        const int _snapshotMaxReturn = 20;

        public async Task<Snapshot<T>[]> GetAsync<T>(string entityId, CancellationToken cancellationToken = default)
        {
            var (start, end) = _claims.GetRange() ?? (0, _snapshotMaxReturn);
            var count   = Math.Clamp(end - start, 0, _snapshotMaxReturn);
            var reverse = bool.TryParse(_claims.QueryParams.GetValueOrDefault("reverse"), out var r) && r;

            var snapshots = await _db.GetSnapshotsAsync<T>(entityId, start, count, reverse, cancellationToken);

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