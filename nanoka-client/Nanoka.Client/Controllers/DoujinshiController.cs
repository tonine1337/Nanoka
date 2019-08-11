using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ipfs.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Core;
using Nanoka.Core.Client;
using Nanoka.Core.Models;

namespace Nanoka.Client.Controllers
{
    [ApiController]
    [Route("doujinshi")]
    public class DoujinshiController : ControllerBase
    {
        readonly IDatabaseClient _client;
        readonly IMapper _mapper;
        readonly IpfsClient _ipfs;

        public DoujinshiController(IDatabaseClient client, IMapper mapper, IpfsClient ipfs)
        {
            _client = client;
            _mapper = mapper;
            _ipfs   = ipfs;
        }

        [HttpPost("search")]
        public async Task<Result<SearchResult<Doujinshi>>> SearchAsync(DoujinshiQuery query)
            => await _client.Doujinshi.SearchAsync(q => query);

        public class UploadDoujinshiRequest
        {
            [FromForm(Name = "doujinshi")]
            public DoujinshiBase Doujinshi { get; set; }

            [FromForm(Name = "variant")]
            public DoujinshiVariantBase Variant { get; set; }

            [FromForm(Name = "file")]
            public IFormFile Archive { get; set; }
        }

        [HttpPost]
        public async Task<Result<UploadState>> UploadAsync([FromForm] UploadDoujinshiRequest request)
        {
            using (var stream = request.Archive.OpenReadStream())
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, false))
                return await _client.Doujinshi.UploadAsync(request.Doujinshi, request.Variant, archive);
        }

        [HttpPut("{id}")]
        public async Task<Result<Doujinshi>> UpdateAsync(Guid id, DoujinshiBase model)
        {
            var doujinshi = await _client.Doujinshi.GetAsync(id);

            if (doujinshi == null)
                return Result.NotFound<Doujinshi>(id);

            _mapper.Map(model, doujinshi);

            await _client.Doujinshi.UpdateAsync(doujinshi);

            return doujinshi;
        }

        [HttpDelete("{id}")]
        public async Task<Result> DeleteAsync(Guid id, [FromQuery] string reason)
        {
            var doujinshi = await _client.Doujinshi.GetAsync(id);

            if (doujinshi == null)
                return Result.NotFound<Doujinshi>(id);

            await _client.Doujinshi.DeleteAsync(doujinshi, reason);

            return Result.Ok();
        }

        public class UploadVariantRequest
        {
            [FromForm(Name = "variant")]
            public DoujinshiVariantBase Variant { get; set; }

            [FromForm(Name = "file")]
            public IFormFile Archive { get; set; }
        }

        [HttpPost("{id}/variants")]
        public async Task<Result<UploadState>> UploadVariantAsync(Guid id, [FromForm] UploadVariantRequest request)
        {
            var doujinshi = await _client.Doujinshi.GetAsync(id);

            if (doujinshi == null)
                return Result.NotFound<Doujinshi>(id);

            using (var stream = request.Archive.OpenReadStream())
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, false))
                return await _client.Doujinshi.UploadVariantAsync(doujinshi, request.Variant, archive);
        }

        [HttpDelete("{id}/variants/{variantId}")]
        public async Task<Result> DeleteVariantAsync(Guid id, Guid variantId)
        {
            var doujinshi = await _client.Doujinshi.GetAsync(id);
            var variant   = doujinshi?.Variants.FirstOrDefault(v => v.Id == variantId);

            if (variant == null)
                return Result.NotFound<DoujinshiVariant>(id, variantId);

            await _client.Doujinshi.DeleteVariantAsync(doujinshi, variant);

            return Result.Ok();
        }

        [HttpGet("{id}/variants/{variantId}/{index}")]
        public async Task<IActionResult> GetImageAsync(Guid id, Guid variantId, int index)
        {
            // todo: cache
            var doujinshi = await _client.Doujinshi.GetAsync(id);
            var variant   = doujinshi?.Variants.FirstOrDefault(v => v.Id == variantId);

            if (variant == null)
                return Result.NotFound<DoujinshiVariant>(id, variantId);

            var links = (await _ipfs.FileSystem.ListFileAsync(variant.Cid))?.Links.ToArray();

            if (links == null || index < 0 || index >= links.Length)
                return Result.NotFound($"Image index '{id}/{variantId}/{index}' is out of range.");

            var node = links[index];

            var stream = await _ipfs.FileSystem.ReadFileAsync(node.Id);

            return new FileStreamResult(stream, MimeTypeMap.GetMimeType(Path.GetExtension(node.Name)));
        }
    }
}