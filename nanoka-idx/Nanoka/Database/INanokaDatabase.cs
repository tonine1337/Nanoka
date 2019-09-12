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

        Task<Book> GetBookAsync(int id, CancellationToken cancellationToken = default);
        Task UpdateBookAsync(Book book, CancellationToken cancellationToken = default);
        Task DeleteBookAsync(int id, CancellationToken cancellationToken = default);

        Task<Image> GetImageAsync(int id, CancellationToken cancellationToken = default);
        Task UpdateImageAsync(Image image, CancellationToken cancellationToken = default);
        Task DeleteImageAsync(int id, CancellationToken cancellationToken = default);

        Task<Snapshot<T>> GetSnapshotAsync<T>(int id, int entityId, CancellationToken cancellationToken = default);
        Task<Snapshot<T>[]> GetSnapshotsAsync<T>(int entityId, CancellationToken cancellationToken = default);
        Task UpdateSnapshotAsync<T>(Snapshot<T> snapshot, CancellationToken cancellationToken = default);
    }
}
