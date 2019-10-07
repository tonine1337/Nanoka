using System.Threading;
using System.Threading.Tasks;
using Nanoka.Models;

namespace Nanoka.Database
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(string id, CancellationToken cancellationToken = default);

        Task<User> GetByNameAsync(string username, CancellationToken cancellationToken = default);

        Task UpdateAsync(User user, CancellationToken cancellationToken = default);

        Task DeleteAsync(User user, CancellationToken cancellationToken = default);
    }
}