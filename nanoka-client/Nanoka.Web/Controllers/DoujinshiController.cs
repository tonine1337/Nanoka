using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nanoka.Core;
using Nanoka.Core.Client;
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

        [HttpPost]
        public async Task<Result<Doujinshi>> CreateDoujinshiAsync(CreateDoujinshiRequest request)
        {
            var now = DateTime.UtcNow;

            var doujinshi = new Doujinshi
            {
                Id         = Guid.NewGuid(),
                UploadTime = now,
                UpdateTime = now,
                Variants   = new List<DoujinshiVariant>()
            };

            _mapper.Map(request.Doujinshi, doujinshi);

            var variant = new DoujinshiVariant
            {
                UploaderId = UserId
            };

            _mapper.Map(request.Variant, variant);

            doujinshi.Variants.Add(variant);

            await _db.IndexAsync(doujinshi);

            return doujinshi;
        }

        async Task CreateSnapshotAsync(Doujinshi doujinshi)
        {
            var snapshot = new Snapshot<Doujinshi>
            {
                Id          = Guid.NewGuid(),
                TargetId    = doujinshi.Id,
                CommitterId = UserId,
                Time        = DateTime.UtcNow,
                Value       = doujinshi
            };

            await _db.IndexSnapshotAsync(snapshot);
        }

        [HttpPut("{id}")]
        public async Task<Result<Doujinshi>> UpdateDoujinshiAsync(Guid id, DoujinshiBase model)
        {
            var doujinshi = await _db.GetDoujinshiAsync(id);

            if (doujinshi == null)
                return Result.NotFound<Doujinshi>(id);

            await CreateSnapshotAsync(doujinshi);

            _mapper.Map(model, doujinshi);

            doujinshi.UpdateTime = DateTime.UtcNow;

            await _db.IndexAsync(doujinshi);

            return doujinshi;
        }

        [HttpDelete("{id}")]
        public async Task<Result<Doujinshi>> DeleteDoujinshiAsync(Guid id)
        {
            var doujinshi = await _db.GetDoujinshiAsync(id);

            if (doujinshi == null)
                return Result.NotFound<Doujinshi>(id);

            await CreateSnapshotAsync(doujinshi);

            await _db.DeleteAsync(doujinshi);

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

            doujinshi.Variants.Add(variant);
            doujinshi.UpdateTime = DateTime.UtcNow;

            await _db.IndexAsync(doujinshi);

            return variant;
        }
    }
}