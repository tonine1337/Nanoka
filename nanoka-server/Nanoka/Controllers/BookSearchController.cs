using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka.Controllers
{
    [ApiController]
    [Route("book")]
    public class BookSearchController : AuthorizedControllerBase
    {
        readonly NanokaOptions _options;
        readonly NanokaDatabase _db;

        public BookSearchController(IOptions<NanokaOptions> options, NanokaDatabase db)
        {
            _options = options.Value;
            _db      = db;
        }

        [HttpPost("search")]
        public async Task<Result<SearchResult<Book>>> SearchAsync(BookQuery query)
        {
            query.Limit = Math.Min(query.Limit, _options.MaxResultCount);

            return await _db.SearchAsync(query);
        }
    }
}