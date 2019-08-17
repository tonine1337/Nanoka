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
    public class DoujinshiUploadTask : UploadTask
    {
        public Doujinshi Doujinshi { get; }

        /// <summary>
        /// Indicates whether this upload creates an entirely new doujinshi or a new variant of an existing doujinshi.
        /// </summary>
        public bool AddVariantOnly { get; }

        public DoujinshiUploadTask(Doujinshi doujinshi) : base(doujinshi.Id)
        {
            Doujinshi = doujinshi;
        }

        public DoujinshiUploadTask(Guid doujinshiId, DoujinshiVariant variant) : base(variant.Id)
        {
            Doujinshi = new Doujinshi
            {
                Id       = doujinshiId,
                Variants = new List<DoujinshiVariant> { variant }
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
                return Result.InvalidUpload<Doujinshi>(id);

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
                case DoujinshiUploadTask doujinshi: return await HandleDoujinshiTaskAsync(doujinshi, stream, final);

                default: throw new NotSupportedException($"Unknown upload task type '{task.GetType().FullName}'.");
            }
        }

        async Task<Result<UploadState>> HandleDoujinshiTaskAsync(DoujinshiUploadTask task, Stream stream, bool final)
        {
            using (stream)
                await task.AddFileAsync(stream);

            // we are uploading more files
            if (!final)
                return task.State;

            try
            {
                var doujinshiId = task.Doujinshi.Id;
                var variantId   = task.Doujinshi.Variants[0].Id;

                // this is an upload for creating a variant of an existing doujinshi
                if (task.AddVariantOnly)
                {
                    using (NanokaLock.EnterAsync(task.Doujinshi.Id))
                    {
                        var doujinshi = await _db.GetDoujinshiAsync(doujinshiId);

                        if (doujinshi == null)
                            return Result.UploadDeleted<Doujinshi>(doujinshiId);

                        await processFilesAsync();

                        await _snapshotManager.SaveAsync(doujinshi, SnapshotEvent.Modification);

                        doujinshi.Variants.Add(task.Doujinshi.Variants[0]);
                        doujinshi.UpdateTime = DateTime.UtcNow;

                        await _db.IndexAsync(doujinshi);
                    }
                }

                // this is an upload for creating a new doujinshi and a default variant
                else
                {
                    await processFilesAsync();

                    var doujinshi = task.Doujinshi;

                    doujinshi.UpdateTime = DateTime.UtcNow;

                    await _db.IndexAsync(doujinshi);
                }

                async Task processFilesAsync()
                {
                    var files = task.GetFiles();

                    for (var i = 0; i < files.Length; i++)
                    {
                        using (var fileStream = files[i].Open(FileMode.Open, FileAccess.Read))
                        {
                            var contentType = _imageProcessor.DetectContentType(fileStream);

                            fileStream.Position = 0;

                            await _storage.AddAsync(new StorageFile
                            {
                                Name        = $"{doujinshiId.ToShortString()}/{variantId.ToShortString()}/{i}",
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