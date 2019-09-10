using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka.Controllers
{
    public class BookUploadTask : UploadTask
    {
        public Book Book { get; }

        /// <summary>
        /// Indicates whether this upload creates an entirely new book or a new variant of an existing book.
        /// </summary>
        public bool AddVariantOnly { get; }

        public BookUploadTask(Book book) : base(book.Id)
        {
            Book = book;
        }

        public BookUploadTask(Guid bookId, BookVariant variant) : base(variant.Id)
        {
            Book = new Book
            {
                Id       = bookId,
                Variants = new List<BookVariant> { variant }
            };

            AddVariantOnly = true;
        }
    }

    [ApiController]
    [Route("uploads")]
    public class UploadController : AuthorizedControllerBase
    {
        readonly UploadManager _uploadManager;
        readonly ImageProcessor _imageProcessor;
        readonly SnapshotManager _snapshotManager;
        readonly NanokaDatabase _db;
        readonly IStorage _storage;

        public UploadController(UploadManager uploadManager,
                                ImageProcessor imageProcessor,
                                SnapshotManager snapshotManager,
                                NanokaDatabase db,
                                IStorage storage)
        {
            _uploadManager   = uploadManager;
            _imageProcessor  = imageProcessor;
            _snapshotManager = snapshotManager;
            _db              = db;
            _storage         = storage;
        }

        [HttpPost("{id}")]
        public async Task<Result<UploadState>> UploadAsync(Guid id, [FromForm(Name = "file")] IFormFile file, [FromQuery] bool final)
        {
            // find upload task
            var task = _uploadManager.GetTask(id);

            if (task == null)
                return Result.InvalidUpload<Book>(id);

            if (file == null)
                return Result.BadRequest($"File not attached in multipart/form-data as '{nameof(file)}'.");

            // load image file
            Stream stream;

            try
            {
                stream = await _imageProcessor.LoadAsync(file);
            }
            catch (Exception e)
            {
                return Result.UnprocessableEntity(e.Message);
            }

            switch (task)
            {
                case BookUploadTask book: return await HandleBookTaskAsync(book, stream, final);

                default: throw new NotSupportedException($"Unknown upload task type '{task.GetType().FullName}'.");
            }
        }

        async Task<Result<UploadState>> HandleBookTaskAsync(BookUploadTask task, Stream stream, bool final)
        {
            using (stream)
                await task.AddFileAsync(stream);

            // we are uploading more files
            if (!final)
                return task.State;

            try
            {
                var bookId  = task.Book.Id;
                var variant = task.Book.Variants[0];

                // this is an upload for creating a variant of an existing book
                if (task.AddVariantOnly)
                {
                    using (NanokaLock.EnterAsync(task.Book.Id))
                    {
                        var book = await _db.GetBookAsync(bookId);

                        if (book == null)
                            return Result.UploadDeleted<Book>(bookId);

                        await processFilesAsync();

                        await _snapshotManager.SaveAsync(book, SnapshotEvent.Modification);

                        book.Variants.Add(task.Book.Variants[0]);
                        book.UpdateTime = DateTime.UtcNow;

                        await _db.IndexAsync(book);
                    }
                }

                // this is an upload for creating a new book and a default variant
                else
                {
                    await processFilesAsync();

                    var book = task.Book;

                    book.UpdateTime = DateTime.UtcNow;

                    await _db.IndexAsync(book);
                }

                async Task processFilesAsync()
                {
                    var files = task.GetFiles();

                    variant.PageCount = files.Length;

                    for (var i = 0; i < files.Length; i++)
                    {
                        using (var fileStream = files[i].Open(FileMode.Open, FileAccess.Read))
                        {
                            var contentType = _imageProcessor.DetectContentType(fileStream);

                            fileStream.Position = 0;

                            await _storage.AddAsync(new StorageFile
                            {
                                Name        = $"{bookId.ToShortString()}/{variant.Id.ToShortString()}/{i}",
                                Stream      = fileStream,
                                ContentType = contentType
                            });
                        }
                    }
                }

                return task.State;
            }
            finally
            {
                // this is the final upload, so remove task
                _uploadManager.RemoveTask(task);
            }
        }
    }
}