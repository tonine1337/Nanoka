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
    [Route("books/uploads")]
    [Authorize]
    public class BookUploadController : ControllerBase
    {
        readonly BookManager _bookManager;
        readonly UploadManager _uploadManager;
        readonly ImageProcessor _imageProcessor;
        readonly IStorage _storage;

        public BookUploadController(BookManager bookManager, UploadManager uploadManager, ImageProcessor imageProcessor,
                                    IStorage storage)
        {
            _bookManager    = bookManager;
            _uploadManager  = uploadManager;
            _imageProcessor = imageProcessor;
            _storage        = storage;
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

                // add info to db
                Book        book;
                BookContent content;

                if (task.Data.Book != null)
                    (book, content) = await _bookManager.CreateAsync(task.Data.Book, task.Data.Content, task);

                else if (task.Data.BookId != null)
                    (book, content) = await _bookManager.AddContentAsync(task.Data.BookId, task.Data.Content, task);

                else
                    return null;

                // add files to storage
                var index = 0;

                foreach (var (_, stream, mediaType) in task.EnumerateFiles())
                {
                    using (stream)
                    {
                        await _storage.WriteAsync(new StorageFile
                        {
                            Name      = $"{book.Id}/{content.Id}/{++index}",
                            Stream    = stream,
                            MediaType = mediaType
                        });
                    }
                }

                return book;
            }
        }
    }
}