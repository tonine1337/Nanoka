using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Nanoka.Core.Models;

namespace Nanoka.Core.Client
{
    public interface IDatabaseClientDoujinshiHandler
    {
        Task<DatabaseUploadTask<Doujinshi>> CreateAsync(DoujinshiBase doujinshi, DoujinshiVariantBase variant, ZipArchive archive, CancellationToken cancellationToken = default);
    }
}