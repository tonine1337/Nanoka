using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        readonly ILogger<SnapshotManager> _logger;

        public SnapshotManager(IOptions<NanokaOptions> options, INanokaDatabase db, UserClaimSet claims, ILogger<SnapshotManager> logger)
        {
            _options = options.Value;
            _db      = db;
            _claims  = claims;
            _logger  = logger;
        }

        public Task<Snapshot<T>> CreatedAsync<T>(SnapshotType type, T value, CancellationToken cancellationToken = default, string committer = null, string reason = null)
            where T : IHasId, IHasEntityType
            => SnapshotInternal(type, SnapshotEvent.Creation, value, cancellationToken, null, committer, reason);

        public Task<Snapshot<T>> ModifiedAsync<T>(SnapshotType type, T value, CancellationToken cancellationToken = default, string committer = null, string reason = null)
            where T : IHasId, IHasEntityType
            => SnapshotInternal(type, SnapshotEvent.Modification, value, cancellationToken, null, committer, reason);

        public Task<Snapshot<T>> DeletedAsync<T>(SnapshotType type, T value, CancellationToken cancellationToken = default, string committer = null, string reason = null)
            where T : IHasId, IHasEntityType
            => SnapshotInternal(type, SnapshotEvent.Deletion, value, cancellationToken, null, committer, reason);

        public Task<Snapshot<T>> RevertedAsync<T>(SnapshotType type, T value, Snapshot<T> previous, CancellationToken cancellationToken = default, string committer = null, string reason = null)
            where T : IHasId, IHasEntityType
            => SnapshotInternal(type, SnapshotEvent.Rollback, value, cancellationToken, previous.Id, committer, reason);

        async Task<Snapshot<T>> SnapshotInternal<T>(SnapshotType type, SnapshotEvent @event, T value, CancellationToken cancellationToken = default,
                                                    string rollbackId = null, string committer = null, string reason = null)
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

            switch (@event)
            {
                case SnapshotEvent.Creation:
                    _logger.LogInformation("Created {0}", snapshot);
                    break;
                case SnapshotEvent.Modification:
                    _logger.LogInformation("Modified {0}", snapshot);
                    break;
                case SnapshotEvent.Deletion:
                    _logger.LogInformation("Deleted {0}", snapshot);
                    break;
                case SnapshotEvent.Rollback:
                    _logger.LogInformation("Reverted {0}", snapshot);
                    break;
            }

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