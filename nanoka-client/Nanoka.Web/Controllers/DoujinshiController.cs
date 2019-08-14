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

        [HttpPost, RequireUnrestricted]
        public Result<UploadState> CreateDoujinshi(CreateDoujinshiRequest request)
        {
            var doujinshi = new Doujinshi
            {
                Id         = Guid.NewGuid(),
                UploadTime = DateTime.UtcNow,

                Variants = new List<DoujinshiVariant>
                {
                    new DoujinshiVariant
                    {
                        Id         = Guid.NewGuid(),
                        Cid        = request.Cid,
                        UploaderId = UserId
                    }
                }
            };

            _mapper.Map(request.Doujinshi, doujinshi);
            _mapper.Map(request.Variant, doujinshi.Variants[0]);

            var worker = _uploadManager.CreateWorker(doujinshi.Id);

            return worker.Start(async (services, token) =>
            {
                await LoadVariantAsync(doujinshi.Variants[0], worker, token);

                doujinshi.UpdateTime = DateTime.UtcNow;

                await _db.IndexAsync(doujinshi, token);

                worker.SetSuccess($"Doujinshi '{doujinshi.Id}' was created.");
            });
        }

        async Task LoadVariantAsync(DoujinshiVariant variant,
                                    UploadWorker worker,
                                    CancellationToken cancellationToken = default)
        {
            worker.SetMessage($"Listing CID '{variant.Cid}'.");

            var variantNode = await _ipfs.FileSystem.ListFileAsync(variant.Cid, cancellationToken);

            if (!variantNode.IsDirectory)
                throw new NotSupportedException($"CID '{variant.Cid}' does not reference a directory.");

            var links = variantNode.Links.ToArray();

            foreach (var node in links)
            {
                if (node.IsDirectory)
                    throw new NotSupportedException($"CID '{variant.Cid}' references another directory '{node.Id}'.");

                if (string.IsNullOrWhiteSpace(node.Name) || node.Name.Length > 64 || Path.GetExtension(node.Name).Length == 0)
                    throw new FormatException($"CID '{variant.Cid}' references a file with an invalid filename '{node.Name}'.");
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
                        throw new NotSupportedException($"File extension '{node.Name}' is invalid for '{mime}'. " +
                                                        $"Use '{MimeTypeMap.GetExtension(mime)}' instead.");

                    if (image.Frames.Count != 1)
                        throw new FormatException($"File '{node.Name}' is not a static image.");

                    if (image.Width == 0 || image.Height == 0)
                        throw new FormatException($"Invalid image dimensions for file '{node.Name}'.");
                }
            }

            variant.PageCount = links.Length;
        }

        [HttpPut("{id}"), RequireUnrestricted]
        public async Task<Result<Doujinshi>> UpdateDoujinshiAsync(Guid id, DoujinshiBase model)
        {
            using (await NanokaLock.EnterAsync(id))
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
        }

        [HttpDelete("{id}"), RequireUnrestricted, RequireReputation(100)]
        public async Task<Result> DeleteDoujinshiAsync(Guid id, [FromQuery] string reason)
        {
            using (await NanokaLock.EnterAsync(id))
            {
                var doujinshi = await _db.GetDoujinshiAsync(id);

                if (doujinshi == null)
                    return Result.NotFound<Doujinshi>(id);

                await CreateSnapshotAsync(doujinshi, SnapshotEvent.Deletion, reason);

                await _db.DeleteAsync(doujinshi);

                return Result.Ok();
            }
        }

        [HttpPost("{id}/variants"), RequireUnrestricted]
        public async Task<Result<UploadState>> CreateVariantAsync(Guid id, CreateDoujinshiVariantRequest request)
        {
            using (await NanokaLock.EnterAsync(id))
            {
                var doujinshi = await _db.GetDoujinshiAsync(id);

                if (doujinshi == null)
                    return Result.NotFound<Doujinshi>(id);
            }

            var variant = new DoujinshiVariant
            {
                Id         = Guid.NewGuid(),
                Cid        = request.Cid,
                UploaderId = UserId
            };

            _mapper.Map(request.Variant, variant);

            var worker = _uploadManager.CreateWorker(variant.Id);

            return worker.Start(async (services, token) =>
            {
                await LoadVariantAsync(variant, worker, token);

                using (await NanokaLock.EnterAsync(id, token))
                {
                    var doujinshi = await _db.GetDoujinshiAsync(id, token);

                    if (doujinshi == null)
                        throw new InvalidOperationException($"Doujinshi '{id}' was deleted.");

                    await CreateSnapshotAsync(doujinshi, SnapshotEvent.Modification);

                    doujinshi.Variants.Add(variant);
                    doujinshi.UpdateTime = DateTime.UtcNow;

                    await _db.IndexAsync(doujinshi, token);
                }

                worker.SetSuccess($"Variant '{id}/{variant.Id}' created.");
            });
        }

        [HttpPut("{id}/variants/{variantId}"), RequireUnrestricted]
        public async Task<Result<DoujinshiVariant>> UpdateVariantAsync(Guid id, Guid variantId, DoujinshiVariantBase model)
        {
            using (await NanokaLock.EnterAsync(id))
            {
                var doujinshi = await _db.GetDoujinshiAsync(id);
                var variant   = doujinshi?.Variants.FirstOrDefault(v => v.Id == variantId);

                if (variant == null)
                    return Result.NotFound<DoujinshiVariant>(id, variantId);

                await CreateSnapshotAsync(doujinshi, SnapshotEvent.Modification);

                _mapper.Map(model, variant);

                doujinshi.UpdateTime = DateTime.UtcNow;

                await _db.IndexAsync(doujinshi);

                return variant;
            }
        }

        [HttpDelete("{id}/variants/{variantId}"), RequireUnrestricted, RequireReputation(100)]
        public async Task<Result> DeleteVariantAsync(Guid id, Guid variantId, [FromQuery] string reason)
        {
            using (await NanokaLock.EnterAsync(id))
            {
                var doujinshi = await _db.GetDoujinshiAsync(id);
                var variant   = doujinshi?.Variants.FirstOrDefault(v => v.Id == variantId);

                if (variant == null)
                    return Result.NotFound<DoujinshiVariant>(id, variantId);

                await CreateSnapshotAsync(doujinshi, SnapshotEvent.Modification, reason);

                doujinshi.Variants.Remove(variant);

                doujinshi.UpdateTime = DateTime.UtcNow;

                await _db.IndexAsync(doujinshi);

                return Result.Ok();
            }
        }
    }
}