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

        public async Task SaveAsync(Book book, SnapshotEvent snapshotEvent, string reason = null)
        {
            var snapshot = new Snapshot<Book>
            {
                Id          = Guid.NewGuid(),
                TargetId    = book.Id,
                CommitterId = _httpContext.ParseUserId(),
                Time        = DateTime.UtcNow,
                Event       = snapshotEvent,
                Target      = SnapshotTarget.Book,
                Reason      = reason,
                Value       = book
            };

            await _db.IndexSnapshotAsync(snapshot);
        }
    }
}