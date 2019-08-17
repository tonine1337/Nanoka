using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nanoka.Models;

namespace Nanoka.Controllers
{
    [ApiController]
    [Route("/")]
    public class DefaultController : AuthorizedControllerBase
    {
        readonly NanokaOptions _options;

        public DefaultController(IOptions<NanokaOptions> options)
        {
            _options = options.Value;
        }

        [HttpGet]
        public Result<DatabaseInfo> GetAsync()
            => new DatabaseInfo
            {
                Version       = NanokaVersion.Latest,
                IpfsBootstrap = _options.IpfsIdentity,
                IpfsSwarmKey  = _options.IpfsSwarmKey?.Replace(" ", "\n")
            };
    }
}