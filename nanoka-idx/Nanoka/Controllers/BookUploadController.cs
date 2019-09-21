using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Models;
using Nanoka.Models.Requests;

namespace Nanoka.Controllers
{
    [ApiController]
    [Route("books/uploads")]
    [Authorize]
    public class BookUploadController : ControllerBase
    {
        readonly BookManager _bookManager;
        readonly UploadManager _uploadManager;
        readonly ImageProcessor _imageProcessor;

        public BookUploadController(BookManager bookManager, UploadManager uploadManager, ImageProcessor imageProcessor)
        {
            _bookManager    = bookManager;
            _uploadManager  = uploadManager;
            _imageProcessor = imageProcessor;
        }

        sealed class BookUpload
        {
            public string BookId;

            public BookBase Book;
            public BookContentBase Content;
        }

        [HttpPost]
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

        [HttpGet("{id}")]
        public Result<UploadState> GetUpload(string id)
            => _uploadManager.GetTask<BookUpload>(id);

        [HttpPost("{id}/files")]
        public async Task<Result<UploadState>> UploadFileAsync(string id, [FromForm(Name = "file")] IFormFile file)
        {
            var task = _uploadManager.GetTask<BookUpload>(id);

            var (stream, mediaType) = await _imageProcessor.LoadAsync(file);

            using (stream)
                await task.AddFileAsync(null, stream, mediaType);

            return task;
        }

        [HttpDelete("{id}")]
        public async Task<Result<Book>> DeleteUploadAsync(string id, [FromQuery] bool commit)
        {
            using (var task = _uploadManager.RemoveTask<BookUpload>(id))
            {
                if (!commit)
                    return Result.Ok();

                if (task.FileCount == 0)
                    return Result.BadRequest("No files were uploaded to be committed.");

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