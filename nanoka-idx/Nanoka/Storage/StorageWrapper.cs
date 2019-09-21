using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nanoka.Storage
{
    /// <summary>
    /// A storage implementation that delegates calls to another storage implementation configured by <see cref="IConfiguration"/>.
    /// </summary>
    public class StorageWrapper : IStorage
    {
        readonly IStorage _impl;

        public StorageWrapper(IServiceProvider services, IConfiguration configuration)
        {
            var type = configuration["Type"];

            if (string.IsNullOrEmpty(type))
                throw new NotSupportedException("Storage type is not configured.");

            switch (type.ToLowerInvariant())
            {
                case "filesystem":
                    _impl = ActivatorUtilities.CreateInstance<LocalStorage>(services, configuration);
                    break;

                case "b2":
                    _impl = ActivatorUtilities.CreateInstance<B2Storage>(services, configuration);
                    break;

                default:
                    throw new NotSupportedException($"Unsupported storage type '{type}'.");
            }
        }

        public Task InitializeAsync(CancellationToken cancellationToken = default) => _impl.InitializeAsync(cancellationToken);
        public Task<StorageFile> ReadAsync(string name, CancellationToken cancellationToken = default) => _impl.ReadAsync(name, cancellationToken);
        public Task<bool> WriteAsync(StorageFile file, CancellationToken cancellationToken = default) => _impl.WriteAsync(file, cancellationToken);
        public Task<bool> DeleteAsync(string name, CancellationToken cancellationToken = default) => _impl.DeleteAsync(name, cancellationToken);

        public void Dispose() => _impl.Dispose();
    }
}