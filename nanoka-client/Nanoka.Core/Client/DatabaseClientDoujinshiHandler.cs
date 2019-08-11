using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Ipfs.Http;
using Nanoka.Core.Models;
using SixLabors.ImageSharp;

namespace Nanoka.Core.Client
{
    public class DatabaseClientDoujinshiHandler : IDatabaseClientDoujinshiHandler
    {
        readonly IDatabaseClient _client;
        readonly IpfsClient _ipfs;

        public DatabaseClientDoujinshiHandler(IDatabaseClient client, IpfsClient ipfs)
        {
            _client = client;
            _ipfs   = ipfs;
        }

        public Task<Doujinshi> GetAsync(Guid id, CancellationToken cancellationToken = default)
            => _client.GetDoujinshiAsync(id, cancellationToken);

        public Task<SearchResult<Doujinshi>> SearchAsync(Func<DoujinshiQuery, DoujinshiQuery> query, CancellationToken cancellationToken = default)
            => _client.SearchDoujinshiAsync(query(new DoujinshiQuery()), cancellationToken);

        public async Task<DatabaseUploadTask<Doujinshi>> CreateAsync(DoujinshiBase doujinshi,
                                                                     DoujinshiVariantBase variant,
                                                                     ZipArchive archive,
                                                                     CancellationToken cancellationToken = default)
        {
            doujinshi.Validate();
            variant.Validate();

            await PrepareVariantArchiveAsync(variant, archive, cancellationToken);

            var state = await _client.CreateDoujinshiAsync(
                new CreateDoujinshiRequest
                {
                    Doujinshi = doujinshi,
                    Variant   = variant
                },
                cancellationToken);

            return new DatabaseUploadTask<Doujinshi>(_client, state);
        }

        async Task PrepareVariantArchiveAsync(DoujinshiVariantBase variant, ZipArchive archive, CancellationToken cancellationToken = default)
        {
            if (archive == null || archive.Mode == ZipArchiveMode.Create)
                throw new NotSupportedException($"{nameof(archive)} is write-only.");

            foreach (var entry in archive.Entries)
            {
                var extension = Path.GetExtension(entry.Name);

                if (string.IsNullOrWhiteSpace(extension))
                    throw new FormatException($"File '{entry.Name}' does not specify an extension.");

                // validate image
                using (var stream = entry.Open())
                using (var image = Image.Load(stream, out var format))
                {
                    var mime = format.DefaultMimeType;

                    if (MimeTypeMap.GetMimeType(extension) != mime)
                        throw new NotSupportedException($"File extension '{entry.Name}' is invalid for '{mime}'.");

                    if (image.Frames.Count != 1)
                        throw new FormatException($"File '{entry.Name}' is not a static image.");

                    if (image.Width == 0 || image.Height == 0)
                        throw new FormatException($"Invalid image dimensions for file '{entry.Name}'.");
                }
            }

            using (var dir = new TempDirectory())
            {
                // extract to temporary directory
                archive.ExtractToDirectory(dir.Path);

                // upload to ipfs
                var node = await _ipfs.FileSystem.AddDirectoryAsync(dir.Path, true, null, cancellationToken);

                variant.Cid = node.Id;
            }
        }

        public async Task UpdateAsync(Doujinshi doujinshi, CancellationToken cancellationToken = default)
        {
            doujinshi.Validate();

            await _client.UpdateDoujinshiAsync(doujinshi.Id, doujinshi, cancellationToken);
        }

        public Task DeleteAsync(Doujinshi doujinshi, string reason, CancellationToken cancellationToken = default)
            => _client.DeleteDoujinshiAsync(doujinshi.Id, reason, cancellationToken);

        public async Task<DatabaseUploadTask<DoujinshiVariant>> CreateVariantAsync(Doujinshi doujinshi, DoujinshiVariantBase variant, ZipArchive archive, CancellationToken cancellationToken = default)
        {
            variant.Validate();

            await PrepareVariantArchiveAsync(variant, archive, cancellationToken);

            var state = await _client.CreateDoujinshiVariantAsync(doujinshi.Id, variant, cancellationToken);

            return new DatabaseUploadTask<DoujinshiVariant>(_client, state);
        }

        public Task DeleteAsync(Doujinshi doujinshi, DoujinshiVariant variant, CancellationToken cancellationToken = default)
            => _client.DeleteDoujinshiVariantAsync(doujinshi.Id, variant.Id, cancellationToken);
    }
}