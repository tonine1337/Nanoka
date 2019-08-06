using System;
using System.Collections.Generic;
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

        async Task CreateSnapshotAsync(Doujinshi doujinshi)
        {
            var snapshot = new Snapshot<Doujinshi>
            {
                Id          = Guid.NewGuid(),
                TargetId    = doujinshi.Id,
                CommitterId = UserId,
                Time        = DateTime.Now,
                Value       = doujinshi
            };

            await _db.IndexSnapshotAsync(snapshot);
        }

        [HttpPut("{id}")]
        public async Task<Result<Doujinshi>> UpdateDoujinshiAsync(Guid id, DoujinshiBase model)
        {
            var now = DateTime.UtcNow;

            var doujinshi = await _db.GetDoujinshiAsync(id);

            if (doujinshi == null)
                doujinshi = new Doujinshi
                {
                    Id         = id,
                    UploadTime = now
                };

            else
                await CreateSnapshotAsync(doujinshi);

            _mapper.Map(model, doujinshi);

            doujinshi.UpdateTime = now;

            await _db.IndexAsync(doujinshi);

            return doujinshi;
        }

        [HttpPost("{id}/variants")]
        public async Task<Result<DoujinshiVariant>> CreateVariantAsync(Guid id, DoujinshiVariantBase model)
        {
            var doujinshi = await _db.GetDoujinshiAsync(id);

            if (doujinshi == null)
                return Result.NotFound<Doujinshi>(id);

            await CreateSnapshotAsync(doujinshi);

            var variant = new DoujinshiVariant
            {
                UploaderId = UserId
            };

            _mapper.Map(model, variant);

            doujinshi.Variants = doujinshi.Variants ?? new List<DoujinshiVariant>();
            doujinshi.Variants.Add(variant);

            await _db.IndexAsync(doujinshi);

            return variant;
        }
    }
}