using System;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Nanoka.Core.Models;

namespace Nanoka.Core.Client
{
    public interface IDatabaseClientDoujinshiHandler
    {
        Task<Doujinshi> GetAsync(Guid id, CancellationToken cancellationToken = default);

        Task<SearchResult<Doujinshi>> SearchAsync(Func<DoujinshiQuery, DoujinshiQuery> query, CancellationToken cancellationToken = default);

        Task<UploadState> UploadAsync(DoujinshiBase doujinshi, DoujinshiVariantBase variant, ZipArchive archive, CancellationToken cancellationToken = default);

        Task UpdateAsync(Doujinshi doujinshi, CancellationToken cancellationToken = default);

        Task DeleteAsync(Doujinshi doujinshi, string reason = null, CancellationToken cancellationToken = default);

        Task<UploadState> UploadVariantAsync(Doujinshi doujinshi, DoujinshiVariantBase variant, ZipArchive archive, CancellationToken cancellationToken = default);

        Task UpdateVariantAsync(Doujinshi doujinshi, DoujinshiVariant variant, CancellationToken cancellationToken = default);

        Task DeleteVariantAsync(Doujinshi doujinshi, DoujinshiVariant variant, string reason = null, CancellationToken cancellationToken = default);
    }
}