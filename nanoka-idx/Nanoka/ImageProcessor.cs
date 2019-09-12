using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

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
            // size check
            if (file.Length >= _options.MaxImageUploadSize)
                throw new ImageProcessorException($"File '{file.FileName}' is too big (must be under {Extensions.GetBytesReadable(_options.MaxImageUploadSize)}).");

            var memory = new MemoryStream((int) file.Length);

            try
            {
                // load onto memory first
                using (var stream = file.OpenReadStream())
                    await stream.CopyToAsync(memory, cancellationToken);

                memory.Position = 0;

                // ensure file is valid image
                Image<Rgba32> image;
                IImageFormat  format;

                try
                {
                    image = Image.Load(memory, out format);
                }
                catch (Exception e)
                {
                    throw new ImageProcessorException($"File '{file.FileName}' is not a recognized image.", e);
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

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public string DetectMediaType(Stream stream)
        {
            var format = Image.DetectFormat(stream);

            if (format == null)
                throw new ImageProcessorException("Could not detect the format of this image.");

            return format.DefaultMimeType;
        }
    }
}