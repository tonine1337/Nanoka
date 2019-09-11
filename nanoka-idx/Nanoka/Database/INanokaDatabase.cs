using System;
using System.Threading;
using System.Threading.Tasks;
using Nanoka.Models;

namespace Nanoka.Database
{
    public interface INanokaDatabase : IDisposable
    {
        Task MigrateAsync(CancellationToken cancellationToken = default);

        Task<User> GetUserAsync(int id, CancellationToken cancellationToken = default);
        Task<User> GetUserAsync(string username, CancellationToken cancellationToken = default);
        Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
        Task DeleteUserAsync(int id, CancellationToken cancellationToken = default);

        Task AddSnapshotAsync<T>(Snapshot<T> snapshot, CancellationToken cancellationToken = default);
    }
}