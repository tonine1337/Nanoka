using System.Threading;
using System.Threading.Tasks;
using Nanoka.Models;

namespace Nanoka.Database
{
    public interface IImageRepository
    {
        Task<Image> GetImageAsync(string id, CancellationToken cancellationToken = default);

        Task UpdateImageAsync(Image image, CancellationToken cancellationToken = default);

        Task DeleteImageAsync(Image image, CancellationToken cancellationToken = default);
    }
}