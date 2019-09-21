using System.Threading;
using System.Threading.Tasks;

namespace Nanoka.Storage
{
    public interface ISupportsUndelete
    {
        Task UndeleteAsync(string name, CancellationToken cancellationToken = default);
    }
}