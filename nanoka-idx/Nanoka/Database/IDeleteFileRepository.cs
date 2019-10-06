using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nanoka.Database
{
    public interface IDeleteFileRepository
    {
        Task AddDeleteFilesAsync(string[] filenames, DateTime softDeleteTime, CancellationToken cancellationToken = default);

        Task RemoveDeleteFileAsync(string[] filenames, CancellationToken cancellationToken = default);

        Task<string[]> GetAndRemoveDeleteFilesAsync(DateTime maxSoftDeleteTime, CancellationToken cancellationToken = default);
    }
}