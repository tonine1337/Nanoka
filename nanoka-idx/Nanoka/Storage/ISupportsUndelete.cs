using System.Threading;
using System.Threading.Tasks;

namespace Nanoka.Storage
{
    public interface ISupportsUndelete
    {
        Task UndeleteAsync(string[] names, CancellationToken cancellationToken = default);
    }
}