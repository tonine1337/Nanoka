using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public async Task<Result<Doujinshi>> GetAsync(Guid id)
        {
            var doujinshi = await _db.GetDoujinshiAsync(id);

            if (doujinshi == null)
                return Result.NotFound<Doujinshi>(id);

            return doujinshi;
        }

        [HttpPost("search")]
        public async Task<Result<SearchResult<Doujinshi>>> SearchAsync(DoujinshiQuery query)
        {
            query.Limit = Math.Min(query.Limit, _options.MaxResultCount);

            return await _db.SearchAsync(query);
        }

        async Task CreateSnapshotAsync(Doujinshi doujinshi, SnapshotEvent snapshotEvent, string reason = null)
        {
            var snapshot = new Snapshot<Doujinshi>
            {
                Id          = Guid.NewGuid(),
                TargetId    = doujinshi.Id,
                CommitterId = UserId,
                Time        = DateTime.UtcNow,
                Event       = snapshotEvent,
                Target      = SnapshotTarget.Doujinshi,
                Reason      = reason,
                Value       = doujinshi
            };

            await _db.IndexSnapshotAsync(snapshot);
        }

        [HttpPost]
        public Result<UploadState<Doujinshi>> CreateDoujinshi(CreateDoujinshiRequest request)
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

            return _uploadManager.CreateWorker<Doujinshi>(async (services, worker, token) =>
            {
                await LoadVariantIpfsAsync(doujinshi.Variants[0], worker, token);

                doujinshi.UpdateTime = DateTime.UtcNow;

                await _db.IndexAsync(doujinshi, token);

                worker.SetSuccess(doujinshi, $"Doujinshi '{doujinshi.Id}' was created.");
            });
        }

        async Task LoadVariantIpfsAsync(DoujinshiVariant variant,
                                        UploadWorker worker,
                                        CancellationToken cancellationToken = default)
        {
            worker.SetProgress(0, $"Listing CID '{variant.Cid}'.");

            var variantNode = await _ipfs.FileSystem.ListFileAsync(variant.Cid, cancellationToken);

            if (!variantNode.IsDirectory)
                throw new NotSupportedException($"CID '{variant.Cid}' does not reference a directory.");

            var links = variantNode.Links.ToArray();

            foreach (var node in links)
            {
                if (node.IsDirectory)
                    throw new NotSupportedException($"CID '{variant.Cid}' references another directory '{node.Id}'.");

                if (node.Name == null || node.Name.Length > 64 || Path.GetExtension(node.Name).Length == 0)
                    throw new FormatException($"CID '{variant.Cid}' references a file with an invalid filename.");
            }

            for (var i = 0; i < links.Length; i++)
            {
                var node = links[i];

                worker.SetProgress(i / (double) links.Length, $"Processing file '{node.Name}'.");

                // ensure file is a valid image
                using (var stream = await _ipfs.FileSystem.ReadFileAsync(node.Id, cancellationToken))
                using (var image = Image.Load(stream, out var format))
                {
                    var mime = format.DefaultMimeType;

                    if (MimeTypeMap.GetMimeType(Path.GetExtension(node.Name)) != mime)
                        throw new NotSupportedException($"File extension '{node.Name}' is invalid for '{mime}'.");

                    if (image.Frames.Count != 1)
                        throw new FormatException($"Not a static image for file '{node.Name}'.");

                    if (image.Width == 0 || image.Height == 0)
                        throw new FormatException($"Invalid image dimensions for file '{node.Name}'.");
                }
            }

            variant.PageCount = links.Length;

            worker.SetProgress(1, $"Pinning {links.Length} files.");

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
        public async Task<Result<Doujinshi>> DeleteDoujinshiAsync(Guid id, [FromQuery] string reason)
        {
            var doujinshi = await _db.GetDoujinshiAsync(id);

            if (doujinshi == null)
                return Result.NotFound<Doujinshi>(id);

            // unpin files
            foreach (var variant in doujinshi.Variants)
                await _ipfs.Pin.RemoveAsync(variant.Cid);

            await CreateSnapshotAsync(doujinshi, SnapshotEvent.Deletion, reason);

            await _db.DeleteAsync(doujinshi);

            return doujinshi;
        }

        [HttpPost("{id}/variants")]
        public async Task<Result<UploadState<DoujinshiVariant>>> CreateVariantAsync(Guid id, DoujinshiVariantBase model)
        {
            var doujinshi = await _db.GetDoujinshiAsync(id);

            if (doujinshi == null)
                return Result.NotFound<Doujinshi>(id);

            var variant = new DoujinshiVariant
            {
                UploaderId = UserId
            };

            _mapper.Map(model, variant);

            return _uploadManager.CreateWorker<DoujinshiVariant>(async (services, worker, token) =>
            {
                await LoadVariantIpfsAsync(variant, worker, token);

                // reload doujinshi because it might have changed
                doujinshi = await _db.GetDoujinshiAsync(id, token);

                if (doujinshi == null)
                    throw new InvalidOperationException($"Doujinshi '{id}' was deleted.");

                await CreateSnapshotAsync(doujinshi, SnapshotEvent.Modification);

                doujinshi.Variants.Add(variant);
                doujinshi.UpdateTime = DateTime.UtcNow;

                await _db.IndexAsync(doujinshi, token);

                worker.SetSuccess(variant, $"Variant '{id}/{doujinshi.Variants.IndexOf(variant)}' created.");
            });
        }

        [HttpPut("{id}/variants/{index}")]
        public async Task<Result<UploadState<DoujinshiVariant>>> UpdateVariantAsync(Guid id, int index, DoujinshiVariantBase model)
        {
            var doujinshi = await _db.GetDoujinshiAsync(id);

            if (doujinshi == null || index < 0 || index >= doujinshi.Variants.Count)
                return Result.NotFound<DoujinshiVariant>(id, index);

            var variant = doujinshi.Variants[index];
            var lastCid = variant.Cid;

            _mapper.Map(model, variant);

            return _uploadManager.CreateWorker<DoujinshiVariant>(async (services, worker, token) =>
            {
                if (variant.Cid != lastCid)
                {
                    await LoadVariantIpfsAsync(variant, worker, token);

                    // reload doujinshi because it might have changed
                    doujinshi = await _db.GetDoujinshiAsync(id, token);

                    if (doujinshi == null)
                        throw new InvalidOperationException($"Doujinshi '{id}' was deleted.");
                }

                await CreateSnapshotAsync(doujinshi, SnapshotEvent.Modification);

                index = doujinshi.Variants.FindIndex(v => v.Cid == lastCid);

                if (index == -1)
                {
                    doujinshi.Variants.Add(variant);
                }
                else
                {
                    // unpin old files
                    await _ipfs.Pin.RemoveAsync(doujinshi.Variants[index].Cid, true, token);

                    doujinshi.Variants[index] = variant;
                }

                doujinshi.UpdateTime = DateTime.UtcNow;

                await _db.IndexAsync(doujinshi, token);

                worker.SetSuccess(variant, $"Variant '{id}/{doujinshi.Variants.IndexOf(variant)}' updated.");
            });
        }

        [HttpDelete("{id}/variants/{index}"), RequireReputation(100)]
        public async Task<Result<DoujinshiVariant>> DeleteVariantAsync(Guid id, int index, [FromQuery] string reason)
        {
            var doujinshi = await _db.GetDoujinshiAsync(id);

            if (doujinshi == null || index < 0 || index >= doujinshi.Variants.Count)
                return Result.NotFound<DoujinshiVariant>(id, index);

            var variant = doujinshi.Variants[index];

            // unpin files
            await _ipfs.Pin.RemoveAsync(variant.Cid);

            await CreateSnapshotAsync(doujinshi, SnapshotEvent.Modification, reason);

            doujinshi.Variants.Remove(variant);

            await _db.IndexAsync(doujinshi);

            return variant;
        }
    }
}