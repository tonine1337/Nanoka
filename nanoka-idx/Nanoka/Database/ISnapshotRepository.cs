using System.Threading;
using System.Threading.Tasks;
using Nanoka.Models;

namespace Nanoka.Database
{
    public interface ISnapshotRepository
    {
        Task<Snapshot<T>> GetSnapshotAsync<T>(string id, string entityId, CancellationToken cancellationToken = default);

        Task<Snapshot<T>[]> GetSnapshotsAsync<T>(string entityId, int start, int count, bool chronological, CancellationToken cancellationToken = default);

        Task UpdateSnapshotAsync<T>(Snapshot<T> snapshot, CancellationToken cancellationToken = default);
    }
}