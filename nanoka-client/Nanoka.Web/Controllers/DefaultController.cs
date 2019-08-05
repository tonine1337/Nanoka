using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nanoka.Core;
using Nanoka.Core.Models;

namespace Nanoka.Web.Controllers
{
    [ApiController]
    public class DefaultController : ControllerBase
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
                Version = NanokaVersion.Latest
            };
    }
}
