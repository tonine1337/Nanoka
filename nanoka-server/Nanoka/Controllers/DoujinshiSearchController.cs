using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka.Controllers
{
    [ApiController]
    [Route("doujinshi")]
    public class DoujinshiSearchController : AuthorizedControllerBase
    {
        readonly NanokaOptions _options;
        readonly NanokaDatabase _db;

        public DoujinshiSearchController(IOptions<NanokaOptions> options, NanokaDatabase db)
        {
            _options = options.Value;
            _db      = db;
        }

        [HttpPost("search")]
        public async Task<Result<SearchResult<Doujinshi>>> SearchAsync(DoujinshiQuery query)
        {
            query.Limit = Math.Min(query.Limit, _options.MaxResultCount);

            return await _db.SearchAsync(query);
        }
    }
}