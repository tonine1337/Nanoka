using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nanoka.Storage
{
    public interface ISoftDeleteQueue
    {
        Task EnqueueAsync(IEnumerable<string> names, DateTime deletedTime, CancellationToken cancellationToken = default);

        Task DequeueAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);

        Task<string[]> DequeueAsync(DateTime maxDeletedTime, CancellationToken cancellationToken = default);
    }
}