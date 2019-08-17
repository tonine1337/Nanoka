using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Database;
using Nanoka.Models;
using Nanoka.Models.Requests;

namespace Nanoka.Controllers
{
    [ApiController]
    [Route("doujinshi")]
    public class DoujinshiVariantController : AuthorizedControllerBase
    {
        readonly NanokaDatabase _db;
        readonly IMapper _mapper;
        readonly RecaptchaValidator _recaptcha;
        readonly UploadManager _uploadManager;
        readonly SnapshotManager _snapshotManager;

        public DoujinshiVariantController(NanokaDatabase db,
                                          IMapper mapper,
                                          RecaptchaValidator recaptcha,
                                          UploadManager uploadManager,
                                          SnapshotManager snapshotManager)
        {
            _db              = db;
            _mapper          = mapper;
            _recaptcha       = recaptcha;
            _uploadManager   = uploadManager;
            _snapshotManager = snapshotManager;
        }

        [HttpGet("{id}/variants/{variantId}")]
        public async Task<Result<DoujinshiVariant>> GetAsync(Guid id, Guid variantId)
        {
            var doujinshi = await _db.GetDoujinshiAsync(id);
            var variant   = doujinshi?.Variants.FirstOrDefault(v => v.Id == variantId);

            if (variant == null)
                return Result.NotFound<DoujinshiVariant>(id, variantId);

            return variant;
        }

        [HttpPost("{id}/variants"), RequireUnrestricted]
        public async Task<Result<UploadState>> CreateAsync(Guid id, CreateDoujinshiVariantRequest request)
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
                UploaderId = UserId
            };

            _mapper.Map(request.Variant, variant);

            return _uploadManager.AddTask(new DoujinshiUploadTask(id, variant)).State;
        }

        [HttpPut("{id}/variants/{variantId}"), RequireUnrestricted]
        public async Task<Result<DoujinshiVariant>> UpdateAsync(Guid id, Guid variantId, DoujinshiVariantBase model)
        {
            using (await NanokaLock.EnterAsync(id))
            {
                var doujinshi = await _db.GetDoujinshiAsync(id);
                var variant   = doujinshi?.Variants.FirstOrDefault(v => v.Id == variantId);

                if (variant == null)
                    return Result.NotFound<DoujinshiVariant>(id, variantId);

                await _snapshotManager.SaveAsync(doujinshi, SnapshotEvent.Modification);

                _mapper.Map(model, variant);

                doujinshi.UpdateTime = DateTime.UtcNow;

                await _db.IndexAsync(doujinshi);

                return variant;
            }
        }

        [HttpDelete("{id}/variants/{variantId}"), RequireUnrestricted, RequireReputation(100)]
        public async Task<Result> DeleteAsync(Guid id, Guid variantId, [FromQuery] string reason, [FromQuery] string token)
        {
            if (!await _recaptcha.ValidateAsync(token))
                return Result.InvalidRecaptchaToken(token);

            using (await NanokaLock.EnterAsync(id))
            {
                var doujinshi = await _db.GetDoujinshiAsync(id);
                var variant   = doujinshi?.Variants.FirstOrDefault(v => v.Id == variantId);

                if (variant == null)
                    return Result.NotFound<DoujinshiVariant>(id, variantId);

                // delete the entire doujinshi if we are the only variant
                if (doujinshi.Variants.Count == 1)
                {
                    await _snapshotManager.SaveAsync(doujinshi, SnapshotEvent.Deletion, reason);

                    await _db.DeleteAsync(doujinshi);
                }
                else
                {
                    await _snapshotManager.SaveAsync(doujinshi, SnapshotEvent.Modification, reason);

                    doujinshi.Variants.Remove(variant);

                    doujinshi.UpdateTime = DateTime.UtcNow;

                    await _db.IndexAsync(doujinshi);
                }

                return Result.Ok();
            }
        }
    }
}