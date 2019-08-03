using System;
using System.Threading;
using System.Threading.Tasks;
using Nanoka.Core.Models;

namespace Nanoka.Core.Client
{
    public interface IDatabaseClient
    {
        Task ConnectAsync(CancellationToken cancellationToken = default);

        Task<Doujinshi> GetDoujinshiAsync(Guid id, CancellationToken cancellationToken = default);
        Task<BooruPost> GetBooruPostAsync(Guid id, CancellationToken cancellationToken = default);
    }
}