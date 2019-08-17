using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using B2Net;
using Microsoft.Extensions.Options;

namespace Nanoka
{
    public class B2Storage : IStorage
    {
        readonly NanokaOptions _options;

        readonly B2Client _client;

        public B2Storage(IOptions<NanokaOptions> options)
        {
            _options = options.Value;

            _client = new B2Client(_options.B2AccountId, _options.B2ApplicationKey);
        }

        string _bucketName;
        string _bucketId;

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            var buckets = await _client.Buckets.GetList(cancellationToken);
            var bucket  = buckets.FirstOrDefault(b => b.BucketName.Equals(_options.B2BucketName, StringComparison.Ordinal));

            if (bucket == null)
                throw new B2StorageException($"Bucket '{_options.B2BucketName}' not found.");

            _bucketName = bucket.BucketName;
            _bucketId   = bucket.BucketId;
        }

        public async Task<Stream> GetAsync(string name, CancellationToken cancellationToken = default)
        {
            var file = await _client.Files.DownloadByName(name, _bucketName, cancellationToken);

            return new MemoryStream(file.FileData);
        }

        public async Task AddAsync(string name, Stream stream, CancellationToken cancellationToken = default)
        {
            byte[] buffer;

            if (stream is MemoryStream memory)
                buffer = memory.ToArray();
            else
                using (var memory2 = new MemoryStream())
                {
                    await stream.CopyToAsync(memory2, cancellationToken);
                    buffer = memory2.ToArray();
                }

            var upload = await _client.Files.GetUploadUrl(_bucketId, cancellationToken);

            await _client.Files.Upload(buffer, name, upload, _bucketId, null, cancellationToken);
        }

        public async Task<bool> RemoveAsync(string name, CancellationToken cancellationToken = default)
        {
            var versions = await _client.Files.GetVersionsWithPrefixOrDelimiter(name, null, name, null, 1, _bucketId, cancellationToken);

            if (versions.Files.Count == 0)
                return false;

            foreach (var file in versions.Files)
                await _client.Files.Delete(file.FileId, name, cancellationToken);

            return true;
        }

        public void Dispose() { }
    }
}