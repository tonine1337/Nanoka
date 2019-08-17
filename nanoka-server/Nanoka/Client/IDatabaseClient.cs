using System;
using System.Threading;
using System.Threading.Tasks;
using Nanoka.Models;

namespace Nanoka.Client
{
    public interface IDatabaseClient : IDisposable
    {
        Task ConnectAsync(CancellationToken cancellationToken = default);

        Task<DatabaseInfo> GetDatabaseInfoAsync(CancellationToken cancellationToken = default);

        Task<Doujinshi> GetDoujinshiAsync(Guid id, CancellationToken cancellationToken = default);
        Task<SearchResult<Doujinshi>> SearchDoujinshiAsync(DoujinshiQuery query, CancellationToken cancellationToken = default);
        Task<UploadState> CreateDoujinshiAsync(CreateDoujinshiRequest request, CancellationToken cancellationToken = default);
        Task<Doujinshi> UpdateDoujinshiAsync(Guid id, DoujinshiBase doujinshi, CancellationToken cancellationToken = default);
        Task DeleteDoujinshiAsync(Guid id, string reason, CancellationToken cancellationToken = default);

        Task<UploadState> CreateDoujinshiVariantAsync(Guid id, CreateDoujinshiVariantRequest request, CancellationToken cancellationToken = default);
        Task<DoujinshiVariant> UpdateDoujinshiVariantAsync(Guid id, Guid variantId, DoujinshiVariantBase variant, CancellationToken cancellationToken = default);
        Task DeleteDoujinshiVariantAsync(Guid id, Guid variantId, string reason, CancellationToken cancellationToken = default);

        Task<BooruPost> GetBooruPostAsync(Guid id, CancellationToken cancellationToken = default);

        Task<UploadState> GetUploadStateAsync(Guid id, CancellationToken cancellationToken = default);
        Task<UploadState> GetNextUploadStateAsync(Guid id, CancellationToken cancellationToken = default);
    }
}