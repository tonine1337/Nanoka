using System.Threading;
using System.Threading.Tasks;
using Nanoka.Models;

namespace Nanoka.Database
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(string id, CancellationToken cancellationToken = default);

        Task<User> GetUserByNameAsync(string username, CancellationToken cancellationToken = default);

        Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);

        Task DeleteUserAsync(User user, CancellationToken cancellationToken = default);
    }
}