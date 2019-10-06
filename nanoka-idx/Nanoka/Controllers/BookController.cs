using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Models;
using Nanoka.Models.Requests;
using Nanoka.Storage;

namespace Nanoka.Controllers
{
    [ApiController]
    [Route("books")]
    [Authorize]
    public class BookController : ControllerBase
    {
        readonly BookManager _bookManager;
        readonly IStorage _storage;
        readonly UploadManager _uploadManager;
        readonly ImageProcessor _imageProcessor;

        public BookController(BookManager bookManager, IStorage storage, UploadManager uploadManager, ImageProcessor imageProcessor)
        {
            _bookManager    = bookManager;
            _storage        = storage;
            _uploadManager  = uploadManager;
            _imageProcessor = imageProcessor;
        }

        [HttpGet("{id}")]
        public async Task<Book> GetAsync(string id)
            => await _bookManager.GetAsync(id);

        [HttpPut("{id}")]
        [UserClaims(unrestricted: true)]
        public async Task<Book> UpdateAsync(string id, BookBase book)
            => await _bookManager.UpdateAsync(id, book);

        [HttpDelete("{id}")]
        [UserClaims(unrestricted: true, reputation: 100, reason: true)]
        [VerifyHuman]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            await _bookManager.DeleteAsync(id);
            return Ok();
        }

        [HttpGet("{id}/snapshots")]
        public async Task<Snapshot<Book>[]> GetSnapshotsAsync(string id)
            => await _bookManager.GetSnapshotsAsync(id);

        [HttpPost("{id}/snapshots/revert")]
        [UserClaims(unrestricted: true, reason: true)]
        public async Task<Book> RevertAsync(string id, RevertEntityRequest request)
            => await _bookManager.RevertAsync(id, request.SnapshotId);

        [HttpPut("{id}/vote")]
        public async Task<Vote> SetVoteAsync(string id, VoteBase vote)
            => await _bookManager.VoteAsync(id, vote.Type);

        [HttpDelete("{id}/vote")]
        public async Task<ActionResult> UnsetVoteAsync(string id)
        {
            await _bookManager.VoteAsync(id, null);
            return Ok();
        }

        [HttpGet("{id}/contents/{contentId}")]
        public async Task<BookContent> GetContentAsync(string id, string contentId)
            => (await _bookManager.GetContentAsync(id, contentId)).content;

        [HttpPut("{id}/contents/{contentId}")]
        public async Task<BookContent> UpdateContentAsync(string id, string contentId, BookContentBase content)
            => await _bookManager.UpdateContentAsync(id, contentId, content);

        [HttpDelete("{id}/contents/{contentId}")]
        [UserClaims(unrestricted: true, reputation: 100, reason: true)]
        [VerifyHuman]
        public async Task<ActionResult> DeleteContentAsync(string id, string contentId)
        {
            await _bookManager.RemoveContentAsync(id, contentId);
            return Ok();
        }

        [HttpGet("{id}/contents/{contentId}/images/{index}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetImageAsync(string id, string contentId, int index)
        {
            var file = await _storage.ReadAsync($"{id}/{contentId}/{index}");

            if (file == null)
                return NotFound();

            return new FileStreamResult(file.Stream, file.MediaType);
        }

        [HttpPost("search")]
        public async Task<SearchResult<Book>> SearchAsync(BookQuery query)
            => await _bookManager.SearchAsync(query);

        sealed class BookUpload
        {
            public string BookId;

            public BookBase Book;
            public BookContentBase Content;
        }

        [HttpPost("uploads")]
        [UserClaims(unrestricted: true)]
        public async Task<UploadState> CreateUploadAsync(CreateNewBookRequest request)
        {
            // creating an entirely new book
            if (request.Book != null)
                return _uploadManager.CreateTask(new BookUpload
                {
                    Book    = request.Book,
                    Content = request.Content
                });

            // adding contents to an existing book
            if (request.BookId != null)
                return _uploadManager.CreateTask(new BookUpload
                {
                    BookId  = (await _bookManager.GetAsync(request.BookId)).Id,
                    Content = request.Content
                });

            return null;
        }

        [HttpGet("uploads/{id}")]
        public UploadState GetUpload(string id)
            => _uploadManager.GetTask<BookUpload>(id);

        [HttpPost("uploads/{id}/files")]
        public async Task<UploadState> UploadFileAsync(string id, [FromForm(Name = "file")] IFormFile file)
        {
            var task = _uploadManager.GetTask<BookUpload>(id);

            var (stream, mediaType) = await _imageProcessor.LoadAsync(file);

            using (stream)
                await task.AddFileAsync(null, stream, mediaType);

            return task;
        }

        [HttpDelete("uploads/{id}")]
        public async Task<ActionResult<Book>> DeleteUploadAsync(string id, [FromQuery] bool commit)
        {
            using (var task = _uploadManager.RemoveTask<BookUpload>(id))
            {
                if (!commit)
                    return Ok();

                if (task.FileCount == 0)
                    return BadRequest("No files were uploaded to be committed.");

                var book = null as Book;

                if (task.Data.Book != null)
                    book = await _bookManager.CreateAsync(task.Data.Book, task.Data.Content, task);

                else if (task.Data.BookId != null)
                    (book, _) = await _bookManager.AddContentAsync(task.Data.BookId, task.Data.Content, task);

                return book;
            }
        }
    }
}