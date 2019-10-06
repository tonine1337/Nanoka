using System.Threading;
using System.Threading.Tasks;
using Nanoka.Models;

namespace Nanoka.Database
{
    public interface IImageRepository
    {
        Task<Image> GetAsync(string id, CancellationToken cancellationToken = default);

        Task UpdateAsync(Image image, CancellationToken cancellationToken = default);

        Task DeleteAsync(Image image, CancellationToken cancellationToken = default);
    }
}