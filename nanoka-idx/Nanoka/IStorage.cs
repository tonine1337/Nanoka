using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nanoka
{
    /// <inheritdoc />
    /// <summary>
    /// An abstract asynchronous storage interface.
    /// </summary>
    public interface IStorage : IDisposable
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);

        Task<StorageFile> GetAsync(string name, CancellationToken cancellationToken = default);

        Task<bool> AddAsync(StorageFile file, CancellationToken cancellationToken = default);

        Task<bool> RemoveAsync(string name, CancellationToken cancellationToken = default);
    }
}