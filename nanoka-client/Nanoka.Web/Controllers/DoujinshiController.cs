using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nanoka.Core;
using Nanoka.Core.Models;
using Nanoka.Web.Database;

namespace Nanoka.Web.Controllers
{
    [ApiController]
    [Route("doujinshi")]
    public class DoujinshiController : AuthorizedControllerBase
    {
        readonly NanokaOptions _options;
        readonly NanokaDatabase _db;
        readonly IMapper _mapper;

        public DoujinshiController(IOptions<NanokaOptions> options, NanokaDatabase db, IMapper mapper)
        {
            _options = options.Value;
            _db      = db;
            _mapper  = mapper;
        }

        [HttpGet("{id}")]
        public async Task<Result<Doujinshi>> GetAsync(Guid id) => await _db.GetDoujinshiAsync(id);

        [HttpPost("search")]
        public async Task<Result<SearchResult<Doujinshi>>> SearchAsync(DoujinshiQuery query)
        {
            query.Limit = Math.Min(query.Limit, _options.MaxResultCount);

            return await _db.SearchAsync(query);
        }

        [HttpPut("{id}")]
        public async Task<Result<Doujinshi>> UpdateAsync(Guid id, DoujinshiBase model)
        {
            var now = DateTime.UtcNow;

            var doujinshi = await _db.GetDoujinshiAsync(id);

            if (doujinshi == null)
            {
                // new doujinshi is being created
                doujinshi = new Doujinshi
                {
                    Id         = id,
                    UploadTime = now
                };
            }

            else
            {
                // we are updating an existing doujinshi
                // create a snapshot first
                var snapshot = new Snapshot<Doujinshi>
                {
                    Id          = Guid.NewGuid(),
                    TargetId    = doujinshi.Id,
                    CommitterId = UserId,
                    Time        = now,
                    Value       = doujinshi
                };

                await _db.IndexSnapshotAsync(snapshot);
            }

            // set new values
            _mapper.Map(model, doujinshi);

            doujinshi.UpdateTime = now;

            // update database
            await _db.IndexAsync(doujinshi);

            return doujinshi;
        }
    }
}