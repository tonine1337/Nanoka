using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nanoka.Client;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka.Controllers
{
    [ApiController]
    [Route("doujinshi")]
    public class DoujinshiController : AuthorizedControllerBase
    {
        readonly NanokaOptions _options;
        readonly NanokaDatabase _db;
        readonly IMapper _mapper;
        readonly UploadManager _uploadManager;

        public DoujinshiController(IOptions<NanokaOptions> options,
                                   NanokaDatabase db,
                                   IMapper mapper,
                                   UploadManager uploadManager)
        {
            _options       = options.Value;
            _db            = db;
            _mapper        = mapper;
            _uploadManager = uploadManager;
        }

        [HttpGet("{id}")]
        public async Task<Result<Doujinshi>> GetAsync(Guid id)
        {
            var doujinshi = await _db.GetDoujinshiAsync(id);

            if (doujinshi == null)
                return Result.NotFound<Doujinshi>(id);

            return doujinshi;
        }

        [HttpPost("search")]
        public async Task<Result<SearchResult<Doujinshi>>> SearchAsync(DoujinshiQuery query)
        {
            query.Limit = Math.Min(query.Limit, _options.MaxResultCount);

            return await _db.SearchAsync(query);
        }

        async Task CreateSnapshotAsync(Doujinshi doujinshi, SnapshotEvent snapshotEvent, string reason = null)
        {
            var snapshot = new Snapshot<Doujinshi>
            {
                Id          = Guid.NewGuid(),
                TargetId    = doujinshi.Id,
                CommitterId = UserId,
                Time        = DateTime.UtcNow,
                Event       = snapshotEvent,
                Target      = SnapshotTarget.Doujinshi,
                Reason      = reason,
                Value       = doujinshi
            };

            await _db.IndexSnapshotAsync(snapshot);
        }

        [HttpPost, RequireUnrestricted]
        public Result<UploadState> CreateDoujinshi(CreateDoujinshiRequest request)
        {
            var doujinshi = new Doujinshi
            {
                Id         = Guid.NewGuid(),
                UploadTime = DateTime.UtcNow,

                Variants = new List<DoujinshiVariant>
                {
                    new DoujinshiVariant
                    {
                        Id         = Guid.NewGuid(),
                        Cid        = request.Cid,
                        UploaderId = UserId
                    }
                }
            };

            _mapper.Map(request.Doujinshi, doujinshi);
            _mapper.Map(request.Variant, doujinshi.Variants[0]);

            var worker = _uploadManager.CreateWorker(doujinshi.Id);

            return worker.Start(async (services, token) =>
            {
                //await LoadVariantAsync(doujinshi.Variants[0], worker, token);

                doujinshi.UpdateTime = DateTime.UtcNow;

                await _db.IndexAsync(doujinshi, token);

                worker.SetSuccess($"Doujinshi '{doujinshi.Id}' was created.");
            });
        }

        [HttpPut("{id}"), RequireUnrestricted]
        public async Task<Result<Doujinshi>> UpdateDoujinshiAsync(Guid id, DoujinshiBase model)
        {
            using (await NanokaLock.EnterAsync(id))
            {
                var doujinshi = await _db.GetDoujinshiAsync(id);

                if (doujinshi == null)
                    return Result.NotFound<Doujinshi>(id);

                await CreateSnapshotAsync(doujinshi, SnapshotEvent.Modification);

                _mapper.Map(model, doujinshi);

                doujinshi.UpdateTime = DateTime.UtcNow;

                await _db.IndexAsync(doujinshi);

                return doujinshi;
            }
        }

        [HttpDelete("{id}"), RequireUnrestricted, RequireReputation(100)]
        public async Task<Result> DeleteDoujinshiAsync(Guid id, [FromQuery] string reason)
        {
            using (await NanokaLock.EnterAsync(id))
            {
                var doujinshi = await _db.GetDoujinshiAsync(id);

                if (doujinshi == null)
                    return Result.NotFound<Doujinshi>(id);

                await CreateSnapshotAsync(doujinshi, SnapshotEvent.Deletion, reason);

                await _db.DeleteAsync(doujinshi);

                return Result.Ok();
            }
        }

        [HttpPost("{id}/variants"), RequireUnrestricted]
        public async Task<Result<UploadState>> CreateVariantAsync(Guid id, CreateDoujinshiVariantRequest request)
        {
            using (await NanokaLock.EnterAsync(id))
            {
                var doujinshi = await _db.GetDoujinshiAsync(id);

                if (doujinshi == null)
                    return Result.NotFound<Doujinshi>(id);
            }

            var variant = new DoujinshiVariant
            {
                Id         = Guid.NewGuid(),
                Cid        = request.Cid,
                UploaderId = UserId
            };

            _mapper.Map(request.Variant, variant);

            var worker = _uploadManager.CreateWorker(variant.Id);

            return worker.Start(async (services, token) =>
            {
                //await LoadVariantAsync(variant, worker, token);

                using (await NanokaLock.EnterAsync(id, token))
                {
                    var doujinshi = await _db.GetDoujinshiAsync(id, token);

                    if (doujinshi == null)
                        throw new InvalidOperationException($"Doujinshi '{id}' was deleted.");

                    await CreateSnapshotAsync(doujinshi, SnapshotEvent.Modification);

                    doujinshi.Variants.Add(variant);
                    doujinshi.UpdateTime = DateTime.UtcNow;

                    await _db.IndexAsync(doujinshi, token);
                }

                worker.SetSuccess($"Variant '{id}/{variant.Id}' created.");
            });
        }

        [HttpPut("{id}/variants/{variantId}"), RequireUnrestricted]
        public async Task<Result<DoujinshiVariant>> UpdateVariantAsync(Guid id, Guid variantId, DoujinshiVariantBase model)
        {
            using (await NanokaLock.EnterAsync(id))
            {
                var doujinshi = await _db.GetDoujinshiAsync(id);
                var variant   = doujinshi?.Variants.FirstOrDefault(v => v.Id == variantId);

                if (variant == null)
                    return Result.NotFound<DoujinshiVariant>(id, variantId);

                await CreateSnapshotAsync(doujinshi, SnapshotEvent.Modification);

                _mapper.Map(model, variant);

                doujinshi.UpdateTime = DateTime.UtcNow;

                await _db.IndexAsync(doujinshi);

                return variant;
            }
        }

        [HttpDelete("{id}/variants/{variantId}"), RequireUnrestricted, RequireReputation(100)]
        public async Task<Result> DeleteVariantAsync(Guid id, Guid variantId, [FromQuery] string reason)
        {
            using (await NanokaLock.EnterAsync(id))
            {
                var doujinshi = await _db.GetDoujinshiAsync(id);
                var variant   = doujinshi?.Variants.FirstOrDefault(v => v.Id == variantId);

                if (variant == null)
                    return Result.NotFound<DoujinshiVariant>(id, variantId);

                await CreateSnapshotAsync(doujinshi, SnapshotEvent.Modification, reason);

                doujinshi.Variants.Remove(variant);

                doujinshi.UpdateTime = DateTime.UtcNow;

                await _db.IndexAsync(doujinshi);

                return Result.Ok();
            }
        }
    }
}