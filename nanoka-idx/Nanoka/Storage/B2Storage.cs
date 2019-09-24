using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bytewizer.Backblaze.Agent;
using Bytewizer.Backblaze.Client;
using Bytewizer.Backblaze.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Nanoka.Storage
{
    public class B2Storage : IStorage
    {
        readonly B2Options _options;
        readonly ILogger<B2Storage> _logger;

        readonly IBackblazeAgent _client;

        public B2Storage(IConfiguration configuration, IMemoryCache cache, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
        {
            _options = configuration.Get<B2Options>();
            _logger  = loggerFactory.CreateLogger<B2Storage>();

            if (_options.MasterKeyId == null || _options.ApplicationKey == null)
                throw new ArgumentException("Backblaze B2 credentials not specified in configuration.");

            _client = new BackblazeAgent(
                new AgentOptions
                {
                    KeyId          = _options.ApplicationKeyId,
                    ApplicationKey = _options.ApplicationKey
                },
                new ApiClient(httpClientFactory.CreateClient(nameof(B2Storage)),
                              loggerFactory.CreateLogger<Bytewizer.Backblaze.Client.Storage>(),
                              cache),
                loggerFactory.CreateLogger<BackblazeAgent>());
        }

        string _bucketName;
        string _bucketId;

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            var buckets = await _client.Buckets.GetAsync(_options.MasterKeyId);
            var bucket  = buckets.Response.Buckets.FirstOrDefault(b => b.BucketName.Equals(_options.BucketName, StringComparison.Ordinal));

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
                using (var memory = new MemoryStream())
                {
                    var response = await _client.DownloadAsync(_bucketName, name, memory, cancellationToken);

                    memory.Position = 0;

                    return new StorageFile
                    {
                        Name      = name,
                        Stream    = memory,
                        MediaType = response.Response.ContentType
                    };
                }
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
                await _client.UploadAsync(
                    new UploadFileByBucketIdRequest(_bucketId, $"http://google.com/{file.Name}")
                    {
                        ContentType = file.MediaType ?? "application/octet-stream"
                    },
                    file.Stream,
                    cancellationToken);

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
            => Task.WhenAll(names.Select(DeleteAsyncInternal));

        async Task<bool> DeleteAsyncInternal(string name)
        {
            try
            {
                var versions = await _client.Files.GetVersionsAsync(_bucketId, name, null, name, 1);

                if (versions.Response.Files.Count == 0)
                    return false;

                foreach (var file in versions.Response.Files)
                    await _client.Files.DeleteAsync(file.FileId, file.FileName);

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