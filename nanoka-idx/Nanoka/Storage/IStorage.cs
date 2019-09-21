using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nanoka.Storage
{
    /// <summary>
    /// An abstract asynchronous storage interface.
    /// </summary>
    public interface IStorage : IDisposable
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);

        Task<StorageFile> ReadAsync(string name, CancellationToken cancellationToken = default);

        Task<bool> WriteAsync(StorageFile file, CancellationToken cancellationToken = default);

        Task DeleteAsync(string[] names, CancellationToken cancellationToken = default);
    }
}