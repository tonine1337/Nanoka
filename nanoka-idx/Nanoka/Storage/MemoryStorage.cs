using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Nanoka.Storage
{
    public class MemoryStorage : IStorage
    {
        readonly ConcurrentDictionary<string, File> _files = new ConcurrentDictionary<string, File>();

        struct File
        {
            public byte[] Buffer;
            public string MediaType;
        }

        public MemoryStorage(IConfiguration configuration) { }

        Task IStorage.InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<StorageFile> ReadAsync(string name, CancellationToken cancellationToken = default)
        {
            if (!_files.TryGetValue(name, out var file))
                return Task.FromResult<StorageFile>(null);

            var buffer = new byte[file.Buffer.Length];
            Buffer.BlockCopy(file.Buffer, 0, buffer, 0, buffer.Length);

            return Task.FromResult(new StorageFile
            {
                Name      = name,
                Stream    = new MemoryStream(buffer),
                MediaType = file.MediaType
            });
        }

        public async Task<bool> WriteAsync(StorageFile file, CancellationToken cancellationToken = default)
        {
            byte[] buffer;

            using (var memory = new MemoryStream())
            {
                await file.Stream.CopyToAsync(memory, cancellationToken);
                buffer = memory.ToArray();
            }

            _files[file.Name] = new File
            {
                Buffer    = buffer,
                MediaType = file.MediaType
            };

            return true;
        }

        public Task DeleteAsync(string[] names, CancellationToken cancellationToken = default)
        {
            foreach (var name in names)
                _files.TryRemove(name, out _);

            return Task.CompletedTask;
        }

        public void Dispose() => _files.Clear();
    }
}