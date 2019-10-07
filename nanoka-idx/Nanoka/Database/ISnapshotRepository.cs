using System.Threading;
using System.Threading.Tasks;
using Nanoka.Models;

namespace Nanoka.Database
{
    public interface ISnapshotRepository
    {
        Task<Snapshot<T>> GetAsync<T>(string id, CancellationToken cancellationToken = default);

        Task<Snapshot<T>[]> GetAsync<T>(string entityId, int start, int count, bool chronological, CancellationToken cancellationToken = default);

        Task UpdateAsync<T>(Snapshot<T> snapshot, CancellationToken cancellationToken = default);
    }
}