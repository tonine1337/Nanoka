using System;
using System.Collections.Generic;
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
        Task DeleteUserAsync(User user, CancellationToken cancellationToken = default);

        Task<Book> GetBookAsync(int id, CancellationToken cancellationToken = default);
        Task UpdateBookAsync(Book book, CancellationToken cancellationToken = default);
        Task DeleteBookAsync(Book book, CancellationToken cancellationToken = default);

        Task<Image> GetImageAsync(int id, CancellationToken cancellationToken = default);
        Task UpdateImageAsync(Image image, CancellationToken cancellationToken = default);
        Task DeleteImageAsync(Image image, CancellationToken cancellationToken = default);

        Task<Snapshot<T>> GetSnapshotAsync<T>(int id, int entityId, CancellationToken cancellationToken = default);
        Task<Snapshot<T>[]> GetSnapshotsAsync<T>(int entityId, CancellationToken cancellationToken = default);
        Task UpdateSnapshotAsync<T>(Snapshot<T> snapshot, CancellationToken cancellationToken = default);

        Task<Vote> GetVoteAsync(int userId, NanokaEntity entity, int entityId, CancellationToken cancellationToken = default);
        Task UpdateVoteAsync(Vote vote, CancellationToken cancellationToken = default);
        Task DeleteVoteAsync(Vote vote, CancellationToken cancellationToken = default);
        Task<int> DeleteVotesAsync(NanokaEntity entity, int entityId, CancellationToken cancellationToken = default);

        Task AddDeleteFilesAsync(IEnumerable<string> filenames, DateTime softDeleteTime, CancellationToken cancellationToken = default);
        Task RemoveDeleteFileAsync(IEnumerable<string> filenames, CancellationToken cancellationToken = default);
        Task<string[]> GetAndRemoveDeleteFilesAsync(DateTime maxSoftDeleteTime, CancellationToken cancellationToken = default);
    }
}