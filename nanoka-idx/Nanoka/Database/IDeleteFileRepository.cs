using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nanoka.Database
{
    public interface IDeleteFileRepository
    {
        Task AddAsync(string[] filenames, DateTime softDeleteTime, CancellationToken cancellationToken = default);

        Task RemoveAsync(string[] filenames, CancellationToken cancellationToken = default);

        Task<string[]> GetAndRemoveAsync(DateTime maxSoftDeleteTime, CancellationToken cancellationToken = default);
    }
}