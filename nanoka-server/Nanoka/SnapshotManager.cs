using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka
{
    public class SnapshotManager
    {
        readonly NanokaDatabase _db;
        readonly HttpContext _httpContext;

        public SnapshotManager(NanokaDatabase db, IHttpContextAccessor httpContextAccessor)
        {
            _db          = db;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public async Task SaveAsync(Doujinshi doujinshi, SnapshotEvent snapshotEvent, string reason = null)
        {
            var snapshot = new Snapshot<Doujinshi>
            {
                Id          = Guid.NewGuid(),
                TargetId    = doujinshi.Id,
                CommitterId = _httpContext.ParseUserId(),
                Time        = DateTime.UtcNow,
                Event       = snapshotEvent,
                Target      = SnapshotTarget.Doujinshi,
                Reason      = reason,
                Value       = doujinshi
            };

            await _db.IndexSnapshotAsync(snapshot);
        }
    }
}