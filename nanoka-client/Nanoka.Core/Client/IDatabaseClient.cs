using System;
using System.Threading;
using System.Threading.Tasks;
using Nanoka.Core.Models;

namespace Nanoka.Core.Client
{
    public interface IDatabaseClient : IDisposable
    {
        IDatabaseClientDoujinshiHandler Doujinshi { get; }

        Task ConnectAsync(CancellationToken cancellationToken = default);

        Task<DatabaseInfo> GetDatabaseInfoAsync(CancellationToken cancellationToken = default);

        Task<Doujinshi> GetDoujinshiAsync(Guid id, CancellationToken cancellationToken = default);
        Task<SearchResult<Doujinshi>> SearchDoujinshiAsync(DoujinshiQuery query, CancellationToken cancellationToken = default);
        Task<UploadState> CreateDoujinshiAsync(CreateDoujinshiRequest request, CancellationToken cancellationToken = default);
        Task<Doujinshi> UpdateDoujinshiAsync(Guid id, DoujinshiBase doujinshi, CancellationToken cancellationToken = default);
        Task DeleteDoujinshiAsync(Guid id, string reason, CancellationToken cancellationToken = default);

        Task<UploadState> CreateDoujinshiVariantAsync(Guid id, DoujinshiVariantBase variant, CancellationToken cancellationToken = default);
        Task DeleteDoujinshiVariantAsync(Guid id, Guid variantId, CancellationToken cancellationToken = default);

        Task<BooruPost> GetBooruPostAsync(Guid id, CancellationToken cancellationToken = default);

        Task<UploadState> GetUploadStateAsync(Guid id, CancellationToken cancellationToken = default);
    }
}