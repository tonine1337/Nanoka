using System.Threading;
using System.Threading.Tasks;
using Nanoka.Models;

namespace Nanoka.Database
{
    public interface IVoteRepository
    {
        Task<Vote> GetAsync(string userId, NanokaEntity entity, string entityId, CancellationToken cancellationToken = default);

        Task UpdateAsync(Vote vote, CancellationToken cancellationToken = default);

        Task DeleteAsync(Vote vote, CancellationToken cancellationToken = default);

        Task<int> DeleteAsync(NanokaEntity entity, string entityId, CancellationToken cancellationToken = default);
    }
}