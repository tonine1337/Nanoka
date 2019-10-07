using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka
{
    public class SnapshotHelper
    {
        readonly ISnapshotRepository _snapshots;
        readonly IUserClaims _claims;
        readonly ILogger<SnapshotHelper> _logger;

        public SnapshotHelper(ISnapshotRepository snapshots, IUserClaims claims, ILogger<SnapshotHelper> logger)
        {
            _snapshots = snapshots;
            _claims    = claims;
            _logger    = logger;
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

            await _snapshots.UpdateAsync(snapshot, cancellationToken);

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

            await _snapshots.UpdateAsync(snapshot, cancellationToken);

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

            await _snapshots.UpdateAsync(snapshot, cancellationToken);

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

            await _snapshots.UpdateAsync(snapshot, cancellationToken);

            _logger.LogInformation("Reverted {0}", snapshot);

            return snapshot;
        }

        const int _snapshotMaxReturn = 20;

        public async Task<Snapshot<T>[]> GetAsync<T>(string entityId, CancellationToken cancellationToken = default)
        {
            var (start, end) = _claims.GetRange() ?? (0, _snapshotMaxReturn);
            var count   = Math.Clamp(end - start, 0, _snapshotMaxReturn);
            var reverse = bool.TryParse(_claims.QueryParams.GetValueOrDefault("reverse"), out var r) && r;

            return await _snapshots.GetAsync<T>(entityId, start, count, reverse, cancellationToken);
        }

        public async Task<Snapshot<T>> GetAsync<T>(string entityId, string id, CancellationToken cancellationToken = default)
            => await _snapshots.GetAsync<T>(id, entityId, cancellationToken);
    }
}