using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Ipfs.Http;
using Nanoka.Core;
using Nanoka.Core.Client;
using SixLabors.ImageSharp;

namespace Nanoka.Client
{
    public class IpfsDoujinshiVariantUploader
    {
        readonly IpfsClient _ipfs;

        public IpfsDoujinshiVariantUploader(IpfsClient ipfs)
        {
            _ipfs = ipfs;
        }

        public async Task InitializeRequestAsync(ZipArchive archive, CreateDoujinshiVariantRequest request, CancellationToken cancellationToken = default)
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

                request.Cid = node.Id;
            }
        }
    }
}