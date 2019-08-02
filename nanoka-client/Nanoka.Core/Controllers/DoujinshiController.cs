using System.IO;
using System.Threading.Tasks;
using Ipfs.Http;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Core.Models;
using Newtonsoft.Json;

namespace Nanoka.Core.Controllers
{
    [ApiController]
    [Route("doujinshi")]
    public class DoujinshiController : ControllerBase
    {
        readonly IpfsClient _ipfs;
        readonly JsonSerializer _serializer;

        public DoujinshiController(IpfsClient ipfs, JsonSerializer serializer)
        {
            _ipfs       = ipfs;
            _serializer = serializer;
        }

        [HttpGet("{cid}")]
        public async Task<Result<Doujinshi>> GetAsync(string cid)
        {
            using (var stream = await _ipfs.FileSystem.ReadFileAsync(cid))
            using (var reader = new StreamReader(stream))
                return _serializer.Deserialize<Doujinshi>(reader);
        }
    }
}