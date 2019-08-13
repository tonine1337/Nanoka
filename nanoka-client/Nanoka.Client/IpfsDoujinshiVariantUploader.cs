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
                var filename  = Path.GetFileName(entry.Name);
                var extension = Path.GetExtension(entry.Name);

                if (entry.Name != filename)
                    throw new NotSupportedException($"File '{filename}' belongs in a nested directory '{Path.GetDirectoryName(entry.Name)}'.\n" +
                                                    "All files must be placed in the top level directory.");

                if (string.IsNullOrWhiteSpace(entry.Name) || entry.Name.Length > 64 || extension.Length == 0)
                    throw new FormatException($"Filename '{entry.Name}' is invalid.");

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

                await _ipfs.Pin.AddAsync(node.Id, true, cancellationToken);
            }
        }
    }
}