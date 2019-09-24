using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using B2Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Nanoka.Storage
{
    public class B2Storage : IStorage
    {
        readonly B2Options _options;
        readonly ILogger<B2Storage> _logger;

        readonly B2Client _client;

        public B2Storage(IConfiguration configuration, ILogger<B2Storage> logger)
        {
            _options = configuration.Get<B2Options>();
            _logger  = logger;

            if (_options.MasterKeyId == null || _options.ApplicationKey == null)
                throw new ArgumentException("Backblaze B2 credentials not specified in configuration.");

            _client = new B2Client(_options.MasterKeyId, _options.ApplicationKey, _options.ApplicationKeyId);
        }

        string _bucketName;
        string _bucketId;

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            var buckets = await _client.Buckets.GetList(cancellationToken);
            var bucket  = buckets.FirstOrDefault(b => b.BucketName.Equals(_options.BucketName, StringComparison.Ordinal));

            if (bucket == null)
                throw new B2StorageException($"Bucket '{_options.BucketName}' not found.");

            _bucketName = bucket.BucketName;
            _bucketId   = bucket.BucketId;

            _logger.LogInformation($"B2 bucket '{_bucketName}' ({_bucketId}).");
        }

        public async Task<StorageFile> ReadAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                var file = await _client.Files.DownloadByName(name, _bucketName, cancellationToken);

                return new StorageFile
                {
                    Name      = file.FileName,
                    Stream    = new MemoryStream(file.FileData),
                    MediaType = file.FileInfo.GetValueOrDefault("type")
                };
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Failed to retrieve file '{name}'.", e);
                return null;
            }
        }

        public async Task<bool> WriteAsync(StorageFile file, CancellationToken cancellationToken = default)
        {
            try
            {
                byte[] buffer;

                if (file.Stream is MemoryStream memory)
                    buffer = memory.ToArray();
                else
                    using (var memory2 = new MemoryStream())
                    {
                        await file.Stream.CopyToAsync(memory2, cancellationToken);
                        buffer = memory2.ToArray();
                    }

                var fileInfo = new Dictionary<string, string>
                {
                    { "type", file.MediaType ?? "application/octet-stream" }
                };

                var upload = await _client.Files.GetUploadUrl(_bucketId, cancellationToken);

                await _client.Files.Upload(buffer, file.Name, upload, _bucketId, fileInfo, cancellationToken);

                _logger.LogInformation($"Wrote file '{file.Name}' ({file.MediaType}).");

                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Could not upload file '{file.Name}'.", e);
                return false;
            }
        }

        public Task DeleteAsync(string[] names, CancellationToken cancellationToken = default)
            => Task.WhenAll(names.Select(n => DeleteAsyncInternal(n, cancellationToken)));

        async Task<bool> DeleteAsyncInternal(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                var versions = await _client.Files.GetVersionsWithPrefixOrDelimiter(name, null, name, null, 1, _bucketId, cancellationToken);

                if (versions.Files.Count == 0)
                    return false;

                foreach (var file in versions.Files)
                    await _client.Files.Delete(file.FileId, name, cancellationToken);

                _logger.LogInformation($"Deleted file '{name}'.");

                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Could not delete file '{name}'.", e);
                return false;
            }
        }

        public void Dispose() { }
    }
}