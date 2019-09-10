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
    [Route("book")]
    public class BookVariantController : AuthorizedControllerBase
    {
        readonly NanokaDatabase _db;
        readonly IMapper _mapper;
        readonly RecaptchaValidator _recaptcha;
        readonly UploadManager _uploadManager;
        readonly SnapshotManager _snapshotManager;
        readonly IStorage _storage;

        public BookVariantController(NanokaDatabase db,
                                     IMapper mapper,
                                     RecaptchaValidator recaptcha,
                                     UploadManager uploadManager,
                                     SnapshotManager snapshotManager,
                                     IStorage storage)
        {
            _db              = db;
            _mapper          = mapper;
            _recaptcha       = recaptcha;
            _uploadManager   = uploadManager;
            _snapshotManager = snapshotManager;
            _storage         = storage;
        }

        [HttpGet("{id}/variants/{variantId}")]
        public async Task<Result<BookVariant>> GetAsync(Guid id, Guid variantId)
        {
            var book    = await _db.GetBookAsync(id);
            var variant = book?.Variants.FirstOrDefault(v => v.Id == variantId);

            if (variant == null)
                return Result.NotFound<BookVariant>(id, variantId);

            return variant;
        }

        [HttpPost("{id}/variants"), RequireUnrestricted]
        public async Task<Result<UploadState>> CreateAsync(Guid id, CreateBookVariantRequest request)
        {
            using (await NanokaLock.EnterAsync(id))
            {
                var book = await _db.GetBookAsync(id);

                if (book == null)
                    return Result.NotFound<Book>(id);
            }

            var variant = new BookVariant
            {
                Id         = Guid.NewGuid(),
                UploaderId = UserId
            };

            _mapper.Map(request.Variant, variant);

            return _uploadManager.AddTask(new BookUploadTask(id, variant)).State;
        }

        [HttpPut("{id}/variants/{variantId}"), RequireUnrestricted]
        public async Task<Result<BookVariant>> UpdateAsync(Guid id, Guid variantId, BookVariantBase model)
        {
            using (await NanokaLock.EnterAsync(id))
            {
                var book    = await _db.GetBookAsync(id);
                var variant = book?.Variants.FirstOrDefault(v => v.Id == variantId);

                if (variant == null)
                    return Result.NotFound<BookVariant>(id, variantId);

                await _snapshotManager.SaveAsync(book, SnapshotEvent.Modification);

                _mapper.Map(model, variant);

                book.UpdateTime = DateTime.UtcNow;

                await _db.IndexAsync(book);

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
                var book    = await _db.GetBookAsync(id);
                var variant = book?.Variants.FirstOrDefault(v => v.Id == variantId);

                if (variant == null)
                    return Result.NotFound<BookVariant>(id, variantId);

                // delete the entire book if we are the only variant
                if (book.Variants.Count == 1)
                {
                    await _snapshotManager.SaveAsync(book, SnapshotEvent.Deletion, reason);

                    await _db.DeleteAsync(book);
                }
                else
                {
                    await _snapshotManager.SaveAsync(book, SnapshotEvent.Modification, reason);

                    book.Variants.Remove(variant);

                    book.UpdateTime = DateTime.UtcNow;

                    await _db.IndexAsync(book);
                }

                return Result.Ok();
            }
        }

        [HttpGet("{id}/variants/{variantId}/images/{index}")]
        public async Task<ActionResult> GetImageAsync(Guid id, Guid variantId, int index)
        {
            var book    = await _db.GetBookAsync(id);
            var variant = book?.Variants.FirstOrDefault(v => v.Id == variantId);

            if (variant == null)
                return Result.NotFound<BookVariant>(id, variantId);

            var file = await _storage.GetAsync($"{id.ToShortString()}/{variantId.ToShortString()}/{index}");

            if (file == null)
                return Result.NotFound($"Could not find page '{index}' in variant '{id}/{variantId}'.");

            return new FileStreamResult(file.Stream, file.ContentType);
        }
    }
}