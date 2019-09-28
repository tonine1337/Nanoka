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
                throw Result.BadRequest("File not attached as multipart/form-data in request.").Exception;

            // size check
            if (file.Length >= _options.MaxImageUploadSize)
                throw Result.UnprocessableEntity($"File '{file.FileName}' is too big (must be under {Extensions.GetBytesReadable(_options.MaxImageUploadSize)}).").Exception;

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
                    throw Result.UnprocessableEntity($"File '{file.FileName}' is not a recognized image: {e.Message}").Exception;
                }

                using (image)
                {
                    if (image.Frames.Count != 1)
                        throw Result.UnprocessableEntity($"File '{file.FileName}' is not a static image.").Exception;
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
                throw Result.UnprocessableEntity("Could not detect the format of this image.").Exception;

            return format.DefaultMimeType;
        }
    }
}