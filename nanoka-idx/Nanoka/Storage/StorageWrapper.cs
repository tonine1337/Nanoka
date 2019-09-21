using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nanoka.Storage
{
    /// <summary>
    /// A storage implementation that delegates calls to another storage implementation configured by <see cref="IConfiguration"/>.
    /// </summary>
    public class StorageWrapper : IStorage, ISupportsUndelete
    {
        static readonly IReadOnlyDictionary<string, Type> _storageImpls =
            typeof(Startup).Assembly
                           .GetTypes()
                           .Where(t => t.IsClass && !t.IsAbstract && typeof(IStorage).IsAssignableFrom(t) && t != typeof(StorageWrapper))
                           .ToDictionary(t => t.Name.Replace("Storage", ""));

        protected readonly IStorage Implementation;

        public StorageWrapper(IServiceProvider services, IConfiguration configuration)
        {
            var type = configuration["Type"];

            if (string.IsNullOrEmpty(type))
                throw new NotSupportedException("Storage type is not configured.");

            if (!_storageImpls.TryGetValue(type, out var storageType))
                throw new NotSupportedException($"Unsupported storage type '{type}'.");

            Implementation = (IStorage) ActivatorUtilities.CreateInstance(services, storageType, configuration);
        }

        public virtual Task InitializeAsync(CancellationToken cancellationToken = default) => Implementation.InitializeAsync(cancellationToken);
        public virtual Task<StorageFile> ReadAsync(string name, CancellationToken cancellationToken = default) => Implementation.ReadAsync(name, cancellationToken);
        public virtual Task<bool> WriteAsync(StorageFile file, CancellationToken cancellationToken = default) => Implementation.WriteAsync(file, cancellationToken);
        public virtual Task<bool> DeleteAsync(string name, CancellationToken cancellationToken = default) => Implementation.DeleteAsync(name, cancellationToken);
        public virtual Task UndeleteAsync(string name, CancellationToken cancellationToken = default) => (Implementation as ISupportsUndelete)?.UndeleteAsync(name, cancellationToken) ?? Task.CompletedTask;

        public virtual void Dispose() => Implementation.Dispose();
    }
}