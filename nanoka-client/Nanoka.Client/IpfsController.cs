using System.IO;
using System.Threading.Tasks;
using Ipfs.Http;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Core;

namespace Nanoka.Client
{
    [ApiController]
    [Route("fs")]
    public class IpfsController : ControllerBase
    {
        readonly IpfsClient _client;

        public IpfsController(IpfsClient client)
        {
            _client = client;
        }

        [HttpGet("{*path}")]
        public async Task<FileStreamResult> GetAsync(string path)
        {
            // split name and ext
            var extension   = Path.GetExtension(path) ?? "";
            var contentType = MimeTypeMap.GetMimeType(extension);

            if (!path.Contains("/"))
                path = path.Substring(0, path.Length - extension.Length);

            var stream = await _client.FileSystem.ReadFileAsync(path);

            return new FileStreamResult(stream, contentType);
        }
    }
}