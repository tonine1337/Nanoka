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

        readonly int _userId;
        readonly string _reason;

        public SnapshotManager(IOptions<NanokaOptions> options, INanokaDatabase db, UserClaimSet claims)
        {
            _options = options.Value;
            _db      = db;

            _userId = claims.Id;
            _reason = claims.Reason;
        }

        public async Task<Snapshot<T>> AddAsync<T>(SnapshotType type, SnapshotEvent @event, T value, CancellationToken cancellationToken = default, int? committer = null, string reason = null)
            where T : IHasId, IHasEntityType
        {
            var snapshot = new Snapshot<T>
            {
                Time        = DateTime.UtcNow,
                CommitterId = committer ?? _userId,
                Type        = type,
                EntityType  = value.Type,
                EntityId    = value.Id,
                Event       = @event,
                Reason      = reason ?? _reason,
                Value       = value
            };

            // require reason for configured event types
            if (type != SnapshotType.System && _options.RequireReasonForEvents.Contains(@event) && string.IsNullOrWhiteSpace(snapshot.Reason))
                throw Result.BadRequest($"{@event} of {value.Type} '{value.Id}': reason must be specified for this action.").Exception;

            await _db.UpdateSnapshotAsync(snapshot, cancellationToken);

            return snapshot;
        }
    }
}