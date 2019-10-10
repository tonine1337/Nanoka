using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Database;
using Nanoka.Models;
using Nanoka.Models.Requests;
using Nanoka.Storage;
using Swashbuckle.AspNetCore.Annotations;

namespace Nanoka.Controllers
{
    [ApiController, Route("books"), Authorize]
    public class BookController : ControllerBase
    {
        readonly IBookRepository _books;
        readonly ILocker _locker;
        readonly IStorage _storage;
        readonly IMapper _mapper;
        readonly UploadManager _uploads;
        readonly SnapshotHelper _snapshots;
        readonly ImageProcessor _image;
        readonly VoteHelper _votes;

        public BookController(IBookRepository books, ILocker locker, IStorage storage, IMapper mapper,
                              UploadManager uploads, SnapshotHelper snapshots, ImageProcessor image, VoteHelper votes)
        {
            _books     = books;
            _locker    = locker;
            _storage   = storage;
            _mapper    = mapper;
            _uploads   = uploads;
            _snapshots = snapshots;
            _image     = image;
            _votes     = votes;
        }

        /// <summary>
        /// Retrieves book information.
        /// </summary>
        /// <param name="id">Book ID.</param>
        [HttpGet("{id}")]
        [SwaggerOperation(OperationId = "getBook")]
        public async Task<ActionResult<Book>> GetAsync(string id)
        {
            var book = await _books.GetAsync(id);

            if (book == null)
                return ResultUtilities.NotFound<Book>(id);

            return book;
        }

        /// <summary>
        /// Updates book information.
        /// </summary>
        /// <param name="id">Book ID.</param>
        /// <param name="model">New book information.</param>
        [HttpPut("{id}")]
        [UserClaims(Unrestricted = true), SwaggerOperation(OperationId = "updateBook")]
        public async Task<ActionResult<Book>> UpdateAsync(string id, BookBase model)
        {
            using (await _locker.EnterAsync(id))
            {
                var book = await _books.GetAsync(id);

                if (book == null)
                    return ResultUtilities.NotFound<Book>(id);

                _mapper.Map(model, book);

                await _books.UpdateAsync(book);
                await _snapshots.ModifiedAsync(book);

                return book;
            }
        }

        /// <summary>
        /// Deletes a book.
        /// </summary>
        /// <param name="id">Book ID.</param>
        [HttpDelete("{id}")]
        [UserClaims(Unrestricted = true, Reputation = 100, Reason = true), SwaggerOperation(OperationId = "deleteBook")]
        [VerifyHuman]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            using (await _locker.EnterAsync(id))
            {
                var book = await _books.GetAsync(id);

                if (book == null)
                    return ResultUtilities.NotFound<Book>(id);

                await _books.DeleteAsync(book);
                await _snapshots.DeletedAsync(book);
                await _storage.DeleteAsync(GetFileNames(book));
            }

            return Ok();
        }

        static string[] GetFileNames(Book book) => book.Contents?.ToArrayMany(c => GetFileNames(book, c)) ?? new string[0];

        static string[] GetFileNames(IHasId book, BookContent content)
        {
            var names = new string[content.PageCount];

            for (var i = 0; i < names.Length; i++)
                names[i] = $"{book.Id}/{content.Id}/{i + 1}";

            return names;
        }

        /// <summary>
        /// Retrieves snapshots of book information.
        /// </summary>
        /// <param name="id">Book ID.</param>
        [HttpGet("{id}/snapshots")]
        [SwaggerOperation(OperationId = "getBookSnapshots")]
        public async Task<Snapshot<Book>[]> GetSnapshotsAsync(string id)
            => await _snapshots.GetAsync<Book>(id);

        /// <summary>
        /// Reverts book information to a previous snapshot.
        /// </summary>
        /// <param name="id">Book ID.</param>
        /// <param name="request">Reversion request.</param>
        [HttpPost("{id}/snapshots/revert")]
        [UserClaims(Unrestricted = true, Reason = true), SwaggerOperation(OperationId = "revertBook")]
        public async Task<ActionResult<Book>> RevertAsync(string id, RevertEntityRequest request)
        {
            using (await _locker.EnterAsync(id))
            {
                var snapshot = await _snapshots.GetAsync<Book>(id, request.SnapshotId);

                if (snapshot == null)
                    return ResultUtilities.NotFound<Snapshot>(id, request.SnapshotId);

                var book = await _books.GetAsync(id);

                if (book != null && snapshot.Value == null)
                {
                    await _books.DeleteAsync(book);
                    await _storage.DeleteAsync(GetFileNames(book));

                    book = null;
                }

                else if (snapshot.Value != null)
                {
                    book = snapshot.Value;

                    await _books.UpdateAsync(book);

                    if (_storage is ISoftDeleteStorage softDeleteStorage)
                        await softDeleteStorage.RestoreAsync(GetFileNames(book));
                }

                await _snapshots.RevertedAsync(snapshot);

                return book;
            }
        }

        /// <summary>
        /// Sets a vote on a book, overwriting one if already set.
        /// </summary>
        /// <param name="id">Book ID.</param>
        /// <param name="model">Vote information.</param>
        [HttpPut("{id}/vote")]
        [SwaggerOperation(OperationId = "addBookVote")]
        public async Task<ActionResult<Vote>> SetVoteAsync(string id, VoteBase model)
        {
            using (await _locker.EnterAsync(id))
            {
                var book = await _books.GetAsync(id);

                if (book == null)
                    return ResultUtilities.NotFound<Book>(id);

                var vote = await _votes.SetAsync(book, model.Type);

                // score is updated
                await _books.UpdateAsync(book);

                return vote;
            }
        }

        /// <summary>
        /// Removes vote from a book, if set.
        /// </summary>
        /// <param name="id">Book ID.</param>
        [HttpDelete("{id}/vote")]
        [SwaggerOperation(OperationId = "clearBookVote")]
        public async Task<ActionResult> UnsetVoteAsync(string id)
        {
            using (await _locker.EnterAsync(id))
            {
                var book = await _books.GetAsync(id);

                if (book == null)
                    return ResultUtilities.NotFound<Book>(id);

                await _votes.SetAsync(book, null);

                // score is updated
                await _books.UpdateAsync(book);

                return Ok();
            }
        }

        /// <summary>
        /// Retrieves book content information.
        /// </summary>
        /// <param name="id">Book ID.</param>
        /// <param name="contentId">Content ID.</param>
        [HttpGet("{id}/contents/{contentId}")]
        [SwaggerOperation(OperationId = "getBookContent")]
        public async Task<ActionResult<BookContent>> GetContentAsync(string id, string contentId)
        {
            var book = await _books.GetAsync(id);

            if (book == null)
                return ResultUtilities.NotFound<Book>(id);

            var content = book.Contents.FirstOrDefault(c => c.Id == contentId);

            if (content == null)
                return ResultUtilities.NotFound<BookContent>(id, contentId);

            return content;
        }

        /// <summary>
        /// Updates book content information.
        /// </summary>
        /// <param name="id">Book ID.</param>
        /// <param name="contentId">Content ID.</param>
        /// <param name="model">New content information.</param>
        [HttpPut("{id}/contents/{contentId}")]
        [SwaggerOperation(OperationId = "updateBookContent")]
        public async Task<ActionResult<BookContent>> UpdateContentAsync(string id, string contentId, BookContentBase model)
        {
            using (await _locker.EnterAsync(id))
            {
                var book = await _books.GetAsync(id);

                if (book == null)
                    return ResultUtilities.NotFound<Book>(id);

                var content = book.Contents.FirstOrDefault(c => c.Id == contentId);

                if (content == null)
                    return ResultUtilities.NotFound<BookContent>(id, contentId);

                _mapper.Map(model, content);

                await _books.UpdateAsync(book);
                await _snapshots.ModifiedAsync(book);

                return content;
            }
        }

        /// <summary>
        /// Deletes a content of a book.
        /// </summary>
        /// <remarks>
        /// If the content being deleted is the only content left in the book, the entire book will be deleted.
        /// </remarks>
        /// <param name="id">Book ID.</param>
        /// <param name="contentId">Content ID.</param>
        [HttpDelete("{id}/contents/{contentId}")]
        [VerifyHuman, UserClaims(Unrestricted = true, Reputation = 100, Reason = true), SwaggerOperation(OperationId = "deleteBookContent")]
        public async Task<ActionResult> DeleteContentAsync(string id, string contentId)
        {
            using (await _locker.EnterAsync(id))
            {
                var book = await _books.GetAsync(id);

                if (book == null)
                    return ResultUtilities.NotFound<Book>(id);

                var content = book.Contents.FirstOrDefault(c => c.Id == contentId);

                if (content == null)
                    return ResultUtilities.NotFound<BookContent>(id, contentId);

                if (book.Contents.Length == 1)
                {
                    // delete the entire book
                    await _books.DeleteAsync(book);
                    await _snapshots.DeletedAsync(book);
                }
                else
                {
                    // remove the content
                    book.Contents = book.Contents.Where(c => c != content).ToArray();

                    await _books.UpdateAsync(book);
                    await _snapshots.ModifiedAsync(book);
                }

                await _storage.DeleteAsync(GetFileNames(book, content));

                return Ok();
            }
        }

        /// <summary>
        /// Gets an image in a book content.
        /// </summary>
        /// <param name="id">Book ID.</param>
        /// <param name="contentId">Content ID.</param>
        /// <param name="index">Page index, starting from one.</param>
        [HttpGet("{id}/contents/{contentId}/images/{index}")]
        [AllowAnonymous, SwaggerOperation(OperationId = "getBookImage")]
        public async Task<ActionResult> GetImageAsync(string id, string contentId, int index)
        {
            var file = await _storage.ReadAsync($"{id}/{contentId}/{index}");

            if (file == null)
                return ResultUtilities.NotFound<StorageFile>(id, contentId, index);

            var stream    = file.Stream;
            var mediaType = file.MediaType;

            return new FileStreamResult(stream, mediaType);
        }

        /// <summary>
        /// Searches for books matching the specified query.
        /// </summary>
        /// <param name="query">Book information query.</param>
        [HttpPost("search")]
        [SwaggerOperation(OperationId = "searchBook")]
        public async Task<SearchResult<Book>> SearchAsync(BookQuery query)
            => await _books.SearchAsync(query);

        sealed class BookUpload
        {
            public string BookId;

            public BookBase Book;
            public BookContentBase Content;
        }

        /// <summary>
        /// Creates a book content upload task.
        /// </summary>
        /// <param name="request">Creation request.</param>
        [HttpPost("uploads")]
        [UserClaims(Unrestricted = true), SwaggerOperation("createBookUpload")]
        public async Task<ActionResult<UploadState>> CreateUploadAsync(CreateNewBookRequest request)
        {
            try
            {
                // creating an entirely new book
                if (request.Book != null)
                    return _uploads.CreateTask(new BookUpload
                    {
                        Book    = request.Book,
                        Content = request.Content
                    });

                // adding contents to an existing book
                if (request.BookId != null)
                {
                    var book = await _books.GetAsync(request.BookId);

                    if (book == null)
                        return ResultUtilities.NotFound<Book>(request.BookId);

                    return _uploads.CreateTask(new BookUpload
                    {
                        BookId  = book.Id,
                        Content = request.Content
                    });
                }
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }

            throw new ArgumentException("Invalid upload state.");
        }

        /// <summary>
        /// Retrieves upload task information.
        /// </summary>
        /// <param name="id">Upload ID.</param>
        [HttpGet("uploads/{id}")]
        [SwaggerOperation(OperationId = "getBookUpload")]
        public ActionResult<UploadState> GetUpload(string id)
        {
            var task = _uploads.GetTask<BookUpload>(id);

            if (task == null)
                return ResultUtilities.NotFound<UploadTask>(id);

            return task;
        }

        /// <summary>
        /// Adds a new image to an upload task.
        /// </summary>
        /// <param name="id">Upload ID.</param>
        /// <param name="file">The file to upload, which must be a valid image.</param>
        [HttpPost("uploads/{id}/images")]
        [SwaggerOperation(OperationId = "uploadBookImage")]
        public async Task<ActionResult<UploadState>> UploadFileAsync(string id, [FromForm(Name = "file")] IFormFile file)
        {
            var task = _uploads.GetTask<BookUpload>(id);

            if (task == null)
                return ResultUtilities.NotFound<UploadTask>(id);

            Stream stream;
            string mediaType;

            try
            {
                (stream, mediaType) = await _image.LoadAsync(file);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            catch (FormatException e)
            {
                return UnprocessableEntity(e.Message);
            }

            try
            {
                using (stream)
                    await task.AddFileAsync(null, stream, mediaType);
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }

            return task;
        }

        /// <summary>
        /// Finishes an upload task.
        /// </summary>
        /// <param name="id">Upload ID.</param>
        /// <param name="commit">Whether to save the upload or discard everything.</param>
        [HttpDelete("uploads/{id}")]
        [SwaggerOperation(OperationId = "finishBookUpload")]
        public async Task<ActionResult<Book>> DeleteUploadAsync(string id, [FromQuery] bool commit)
        {
            using (var task = _uploads.RemoveTask<BookUpload>(id))
            {
                if (task == null)
                    return ResultUtilities.NotFound<UploadTask>(id);

                if (!commit)
                    return Ok();

                if (task.FileCount == 0)
                    return BadRequest("No files were uploaded to be committed.");

                if (task.Data.Book != null)
                {
                    var book    = _mapper.Map<Book>(task.Data.Book);
                    var content = _mapper.Map<BookContent>(task.Data.Content);

                    content.Id        = Snowflake.New;
                    content.PageCount = task.FileCount;

                    book.Contents = new[] { content };

                    await _books.UpdateAsync(book);
                    await _snapshots.CreatedAsync(book);

                    await UploadContentAsync(book, content, task);

                    return book;
                }

                if (task.Data.BookId != null)
                    using (await _locker.EnterAsync(task.Data.BookId))
                    {
                        var book = await _books.GetAsync(task.Data.BookId);

                        if (book == null)
                            return ResultUtilities.NotFound<Book>(task.Data.BookId);

                        var content = _mapper.Map<BookContent>(task.Data.Content);

                        content.Id        = Snowflake.New;
                        content.PageCount = task.FileCount;

                        book.Contents = book.Contents.Append(content).ToArray();

                        await _books.UpdateAsync(book);
                        await _snapshots.ModifiedAsync(book);

                        await UploadContentAsync(book, content, task);

                        return book;
                    }

                throw new InvalidOperationException("Invalid upload state.");
            }
        }

        async Task UploadContentAsync(IHasId book, BookContent content, UploadTask task)
        {
            var names = GetFileNames(book, content);

            var array = task.EnumerateFiles().ToArray();

            for (var i = 0; i < array.Length; i++)
            {
                var (_, s, m) = array[i];

                await _storage.WriteAsync(new StorageFile
                {
                    Name      = names[i],
                    Stream    = s,
                    MediaType = m
                });
            }
        }
    }
}