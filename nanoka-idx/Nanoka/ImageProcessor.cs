using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace Nanoka
{
    public class ImageProcessor
    {
        readonly NanokaOptions _options;

        public ImageProcessor(IOptions<NanokaOptions> options)
        {
            _options = options.Value;
        }

        public async Task<(Stream stream, string mediaType)> LoadAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file), "File not attached as multipart/form-data in request.");

            // size check
            if (file.Length >= _options.MaxImageUploadSize)
                throw new ArgumentException($"File '{file.FileName}' is too big (must be under {Extensions.GetBytesReadable(_options.MaxImageUploadSize)}).", nameof(file));

            var memory = new MemoryStream((int) file.Length);

            try
            {
                // load onto memory first
                using (var stream = file.OpenReadStream())
                    await stream.CopyToAsync(memory, cancellationToken);

                memory.Position = 0;

                // ensure file is valid image
                Image        image;
                IImageFormat format;

                try
                {
                    image = Image.Load(memory, out format);
                }
                catch (Exception e)
                {
                    throw new FormatException($"File '{file.FileName}' is not a recognized image: {e.Message}");
                }

                using (image)
                {
                    if (image.Frames.Count != 1)
                        throw new FormatException($"File '{file.FileName}' is not a static image.");
                }

                memory.Position = 0;

                return (memory, format.DefaultMimeType);
            }
            catch
            {
                memory.Dispose();
                throw;
            }
        }
    }
}