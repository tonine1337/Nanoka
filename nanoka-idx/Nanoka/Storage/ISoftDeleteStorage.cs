using System.Threading;
using System.Threading.Tasks;

namespace Nanoka.Storage
{
    public interface ISoftDeleteStorage : IStorage
    {
        Task RestoreAsync(string[] names, CancellationToken cancellationToken = default);
    }
}