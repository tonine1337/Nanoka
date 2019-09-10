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
    [Route("book")]
    public class BookController : AuthorizedControllerBase
    {
        readonly NanokaDatabase _db;
        readonly IMapper _mapper;
        readonly RecaptchaValidator _recaptcha;
        readonly UploadManager _uploadManager;
        readonly SnapshotManager _snapshotManager;

        public BookController(NanokaDatabase db, IMapper mapper, RecaptchaValidator recaptcha, UploadManager uploadManager,
                              SnapshotManager snapshotManager)
        {
            _db              = db;
            _mapper          = mapper;
            _recaptcha       = recaptcha;
            _uploadManager   = uploadManager;
            _snapshotManager = snapshotManager;
        }

        [HttpGet("{id}")]
        public async Task<Result<Book>> GetAsync(Guid id)
        {
            var book = await _db.GetBookAsync(id);

            if (book == null)
                return Result.NotFound<Book>(id);

            return book;
        }

        [HttpPost, RequireUnrestricted]
        public Result<UploadState> Create(CreateBookRequest request)
        {
            var book = new Book
            {
                Id         = Guid.NewGuid(),
                UploadTime = DateTime.UtcNow,

                Variants = new List<BookVariant>
                {
                    new BookVariant
                    {
                        Id         = Guid.NewGuid(),
                        UploaderId = UserId
                    }
                }
            };

            _mapper.Map(request.Book, book);
            _mapper.Map(request.Variant, book.Variants[0]);

            return _uploadManager.AddTask(new BookUploadTask(book)).State;
        }

        [HttpPut("{id}"), RequireUnrestricted]
        public async Task<Result<Book>> UpdateAsync(Guid id, BookBase model)
        {
            using (await NanokaLock.EnterAsync(id))
            {
                var book = await _db.GetBookAsync(id);

                if (book == null)
                    return Result.NotFound<Book>(id);

                await _snapshotManager.SaveAsync(book, SnapshotEvent.Modification);

                _mapper.Map(model, book);

                book.UpdateTime = DateTime.UtcNow;

                await _db.IndexAsync(book);

                return book;
            }
        }

        [HttpDelete("{id}"), RequireUnrestricted, RequireReputation(100)]
        public async Task<Result> DeleteAsync(Guid id, [FromQuery] string reason, [FromQuery] string token)
        {
            if (!await _recaptcha.ValidateAsync(token))
                return Result.InvalidRecaptchaToken(token);

            using (await NanokaLock.EnterAsync(id))
            {
                var book = await _db.GetBookAsync(id);

                if (book == null)
                    return Result.NotFound<Book>(id);

                await _snapshotManager.SaveAsync(book, SnapshotEvent.Deletion, reason);

                await _db.DeleteAsync(book);

                return Result.Ok();
            }
        }
    }
}