using System;
using System.IO;
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

        Task<Stream> GetAsync(string name, CancellationToken cancellationToken = default);

        Task AddAsync(string name, Stream stream, CancellationToken cancellationToken = default);

        Task<bool> RemoveAsync(string name, CancellationToken cancellationToken = default);
    }
}