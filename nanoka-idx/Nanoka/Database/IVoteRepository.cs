using System.Threading;
using System.Threading.Tasks;
using Nanoka.Models;

namespace Nanoka.Database
{
    public interface IVoteRepository
    {
        Task<Vote> GetVoteAsync(string userId, NanokaEntity entity, string entityId, CancellationToken cancellationToken = default);

        Task UpdateVoteAsync(Vote vote, CancellationToken cancellationToken = default);

        Task DeleteVoteAsync(Vote vote, CancellationToken cancellationToken = default);

        Task<int> DeleteVotesAsync(NanokaEntity entity, string entityId, CancellationToken cancellationToken = default);
    }
}