using System;
using System.Threading;
using System.Threading.Tasks;
using Nanoka.Core.Models;

namespace Nanoka.Core.Client
{
    public interface IDatabaseClient : IDisposable
    {
        Task ConnectAsync(CancellationToken cancellationToken = default);

        Task<Doujinshi> GetDoujinshiAsync(Guid id, CancellationToken cancellationToken = default);
        Task<SearchResult<Doujinshi>> SearchDoujinshiAsync(DoujinshiQuery query, CancellationToken cancellationToken = default);
        Task<UploadState<Doujinshi>> CreateDoujinshiAsync(CreateDoujinshiRequest request, CancellationToken cancellationToken = default);
        Task<Doujinshi> UpdateDoujinshiAsync(Guid id, DoujinshiBase doujinshi, CancellationToken cancellationToken = default);
        Task<Doujinshi> DeleteDoujinshiAsync(Guid id, string reason, CancellationToken cancellationToken = default);

        Task<UploadState<DoujinshiVariant>> CreateDoujinshiVariantAsync(Guid id, DoujinshiVariantBase variant, CancellationToken cancellationToken = default);
        Task<UploadState<DoujinshiVariant>> UpdateDoujinshiVariantAsync(Guid id, int index, DoujinshiVariantBase variant, CancellationToken cancellationToken = default);
        Task<DoujinshiVariant> DeleteDoujinshiVariantAsync(Guid id, int index, CancellationToken cancellationToken = default);

        Task<BooruPost> GetBooruPostAsync(Guid id, CancellationToken cancellationToken = default);

        Task<UploadState<T>> GetUploadStateAsync<T>(Guid id, CancellationToken cancellationToken = default);
    }
}