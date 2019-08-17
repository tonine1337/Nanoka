using System;
using System.Collections.Generic;
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
    public class DoujinshiController : AuthorizedControllerBase
    {
        readonly NanokaDatabase _db;
        readonly IMapper _mapper;
        readonly RecaptchaValidator _recaptcha;
        readonly UploadManager _uploadManager;
        readonly SnapshotManager _snapshotManager;

        public DoujinshiController(NanokaDatabase db,
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

        [HttpGet("{id}")]
        public async Task<Result<Doujinshi>> GetAsync(Guid id)
        {
            var doujinshi = await _db.GetDoujinshiAsync(id);

            if (doujinshi == null)
                return Result.NotFound<Doujinshi>(id);

            return doujinshi;
        }

        [HttpPost, RequireUnrestricted]
        public Result<UploadState> Create(CreateDoujinshiRequest request)
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
                        UploaderId = UserId
                    }
                }
            };

            _mapper.Map(request.Doujinshi, doujinshi);
            _mapper.Map(request.Variant, doujinshi.Variants[0]);

            return _uploadManager.AddTask(new DoujinshiUploadTask(doujinshi)).State;
        }

        [HttpPut("{id}"), RequireUnrestricted]
        public async Task<Result<Doujinshi>> UpdateAsync(Guid id, DoujinshiBase model)
        {
            using (await NanokaLock.EnterAsync(id))
            {
                var doujinshi = await _db.GetDoujinshiAsync(id);

                if (doujinshi == null)
                    return Result.NotFound<Doujinshi>(id);

                await _snapshotManager.SaveAsync(doujinshi, SnapshotEvent.Modification);

                _mapper.Map(model, doujinshi);

                doujinshi.UpdateTime = DateTime.UtcNow;

                await _db.IndexAsync(doujinshi);

                return doujinshi;
            }
        }

        [HttpDelete("{id}"), RequireUnrestricted, RequireReputation(100)]
        public async Task<Result> DeleteAsync(Guid id, [FromQuery] string reason, [FromQuery] string token)
        {
            if (!await _recaptcha.ValidateAsync(token))
                return Result.InvalidRecaptchaToken(token);

            using (await NanokaLock.EnterAsync(id))
            {
                var doujinshi = await _db.GetDoujinshiAsync(id);

                if (doujinshi == null)
                    return Result.NotFound<Doujinshi>(id);

                await _snapshotManager.SaveAsync(doujinshi, SnapshotEvent.Deletion, reason);

                await _db.DeleteAsync(doujinshi);

                return Result.Ok();
            }
        }
    }
}