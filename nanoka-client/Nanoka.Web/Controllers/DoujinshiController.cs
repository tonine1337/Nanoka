using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ipfs.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nanoka.Core;
using Nanoka.Core.Client;
using Nanoka.Core.Models;
using Nanoka.Web.Database;
using SixLabors.ImageSharp;

namespace Nanoka.Web.Controllers
{
    [ApiController]
    [Route("doujinshi")]
    public class DoujinshiController : AuthorizedControllerBase
    {
        readonly NanokaOptions _options;
        readonly NanokaDatabase _db;
        readonly IMapper _mapper;
        readonly UploadManager _uploadManager;
        readonly IpfsClient _ipfs;

        public DoujinshiController(IOptions<NanokaOptions> options,
                                   NanokaDatabase db,
                                   IMapper mapper,
                                   UploadManager uploadManager,
                                   IpfsClient ipfs)
        {
            _options       = options.Value;
            _db            = db;
            _mapper        = mapper;
            _uploadManager = uploadManager;
            _ipfs          = ipfs;
        }

        [HttpGet("{id}")]
        public async Task<Result<Doujinshi>> GetAsync(Guid id) => await _db.GetDoujinshiAsync(id);

        [HttpPost("search")]
        public async Task<Result<SearchResult<Doujinshi>>> SearchAsync(DoujinshiQuery query)
        {
            query.Limit = Math.Min(query.Limit, _options.MaxResultCount);

            return await _db.SearchAsync(query);
        }

        async Task CreateSnapshotAsync(Doujinshi doujinshi, SnapshotEvent snapshotEvent)
        {
            var snapshot = new Snapshot<Doujinshi>
            {
                Id          = Guid.NewGuid(),
                TargetId    = doujinshi.Id,
                CommitterId = UserId,
                Time        = DateTime.UtcNow,
                Event       = snapshotEvent,
                Target      = SnapshotTarget.Doujinshi,
                Value       = doujinshi
            };

            await _db.IndexSnapshotAsync(snapshot);
        }

        [HttpPost]
        public Result<UploadWorker> CreateDoujinshi(CreateDoujinshiRequest request)
        {
            var doujinshi = new Doujinshi
            {
                Id         = Guid.NewGuid(),
                UploadTime = DateTime.UtcNow,

                Variants = new List<DoujinshiVariant>
                {
                    new DoujinshiVariant
                    {
                        UploaderId = UserId
                    }
                }
            };

            _mapper.Map(request.Doujinshi, doujinshi);
            _mapper.Map(request.Variant, doujinshi.Variants[0]);

            return _uploadManager.CreateWorker(async (services, token) =>
            {
                await LoadVariantIpfsAsync(doujinshi.Variants[0], token);

                doujinshi.UpdateTime = DateTime.UtcNow;

                await _db.IndexAsync(doujinshi, token);
            });
        }

        async Task LoadVariantIpfsAsync(DoujinshiVariant variant,
                                        CancellationToken cancellationToken = default)
        {
            var variantNode = await _ipfs.FileSystem.ListFileAsync(variant.Cid, cancellationToken);

            if (!variantNode.IsDirectory)
                throw new NotSupportedException($"CID '{variant.Cid}' does not reference a directory.");

            var count = 0;

            foreach (var node in variantNode.Links)
            {
                var extension = Path.GetExtension(node.Name);

                if (node.Name == null || node.Name.Length > 64 || extension == null)
                    throw new FormatException($"CID '{variant.Cid}' references a file with an invalid filename.");

                if (node.IsDirectory)
                    throw new NotSupportedException($"CID '{variant.Cid}' references another directory '{node.Id}'.");

                // ensure file is a valid image
                using (var stream = await _ipfs.FileSystem.ReadFileAsync(node.Id, cancellationToken))
                using (var image = Image.Load(stream, out var format))
                {
                    var mime = format.DefaultMimeType;

                    if (MimeTypeMap.GetMimeType(extension) != mime)
                        throw new NotSupportedException($"File extension '{node.Name}' is invalid for '{mime}'.");

                    if (image.Frames.Count != 1)
                        throw new FormatException($"Not a static image for file '{node.Name}'.");

                    if (image.Width == 0 || image.Height == 0)
                        throw new FormatException($"Invalid image dimensions for file '{node.Name}'.");
                }

                ++count;
            }

            variant.PageCount = count;

            // make the files always available
            await _ipfs.Pin.AddAsync(variantNode.Id, true, cancellationToken);
        }

        [HttpPut("{id}")]
        public async Task<Result<Doujinshi>> UpdateDoujinshiAsync(Guid id, DoujinshiBase model)
        {
            var doujinshi = await _db.GetDoujinshiAsync(id);

            if (doujinshi == null)
                return Result.NotFound<Doujinshi>(id);

            await CreateSnapshotAsync(doujinshi, SnapshotEvent.Modification);

            _mapper.Map(model, doujinshi);

            doujinshi.UpdateTime = DateTime.UtcNow;

            await _db.IndexAsync(doujinshi);

            return doujinshi;
        }

        [HttpDelete("{id}"), RequireReputation(100)]
        public async Task<Result<Doujinshi>> DeleteDoujinshiAsync(Guid id)
        {
            var doujinshi = await _db.GetDoujinshiAsync(id);

            if (doujinshi == null)
                return Result.NotFound<Doujinshi>(id);

            // unpin files
            foreach (var variant in doujinshi.Variants)
                await _ipfs.Pin.RemoveAsync(variant.Cid);

            await CreateSnapshotAsync(doujinshi, SnapshotEvent.Deletion);

            await _db.DeleteAsync(doujinshi);

            return doujinshi;
        }

        [HttpPost("{id}/variants")]
        public async Task<Result<UploadWorker>> CreateVariantAsync(Guid id, DoujinshiVariantBase model)
        {
            var doujinshi = await _db.GetDoujinshiAsync(id);

            if (doujinshi == null)
                return Result.NotFound<Doujinshi>(id);

            var variant = new DoujinshiVariant
            {
                UploaderId = UserId
            };

            _mapper.Map(model, variant);

            return _uploadManager.CreateWorker(async (services, token) =>
            {
                await LoadVariantIpfsAsync(variant, token);

                // reload doujinshi because it might have changed
                doujinshi = await _db.GetDoujinshiAsync(id, token);

                if (doujinshi == null)
                    throw new InvalidOperationException($"Doujinshi '{id}' was deleted.");

                await CreateSnapshotAsync(doujinshi, SnapshotEvent.Modification);

                doujinshi.Variants.Add(variant);
                doujinshi.UpdateTime = DateTime.UtcNow;

                await _db.IndexAsync(doujinshi, token);
            });
        }

        [HttpPut("{id}/variants/{index}"), RequireReputation(100)]
        public async Task<Result<DoujinshiVariant>> DeleteVariantAsync(Guid id, int index)
        {
            var doujinshi = await _db.GetDoujinshiAsync(id);

            if (index < 0 || index >= doujinshi.Variants.Count)
                return Result.NotFound<DoujinshiVariant>(id, index);

            var variant = doujinshi.Variants[index];

            // unpin files
            await _ipfs.Pin.RemoveAsync(variant.Cid);

            await CreateSnapshotAsync(doujinshi, SnapshotEvent.Modification);

            doujinshi.Variants.Remove(variant);

            await _db.IndexAsync(doujinshi);

            return variant;
        }
    }
}