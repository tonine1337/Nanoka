using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
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
        readonly IpfsClient _ipfs;
        readonly IpfsDoujinshiVariantUploader _uploader;

        public DoujinshiController(IDatabaseClient client, IpfsClient ipfs, IpfsDoujinshiVariantUploader uploader)
        {
            _client   = client;
            _ipfs     = ipfs;
            _uploader = uploader;
        }

        [HttpPost("search")]
        public async Task<Result<SearchResult<Doujinshi>>> SearchAsync(DoujinshiQuery query)
            => await _client.SearchDoujinshiAsync(query);

        public class UploadDoujinshiRequest
        {
            [FromForm(Name = "doujinshi"), ModelBinder(typeof(FileModelBinder))]
            public DoujinshiBase Doujinshi { get; set; }

            [FromForm(Name = "variant"), ModelBinder(typeof(FileModelBinder))]
            public DoujinshiVariantBase Variant { get; set; }

            [FromForm(Name = "file")]
            public IFormFile Archive { get; set; }
        }

        [HttpPost]
        public async Task<Result<UploadState>> UploadAsync([FromForm] UploadDoujinshiRequest request)
        {
            if (request.Archive == null)
                return Result.BadRequest("File is not selected.");

            var dbRequest = new CreateDoujinshiRequest
            {
                Doujinshi = request.Doujinshi,
                Variant   = request.Variant
            };

            using (var stream = request.Archive.OpenReadStream())
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, false))
                await _uploader.InitializeRequestAsync(archive, dbRequest);

            return await _client.CreateDoujinshiAsync(dbRequest);
        }

        [HttpPut("{id}")]
        public async Task<Result<Doujinshi>> UpdateAsync(Guid id, DoujinshiBase doujinshi)
            => await _client.UpdateDoujinshiAsync(id, doujinshi);

        [HttpDelete("{id}")]
        public async Task<Result> DeleteAsync(Guid id, [FromQuery] string reason)
        {
            await _client.DeleteDoujinshiAsync(id, reason);

            return Result.Ok();
        }

        public class UploadVariantRequest
        {
            [FromForm(Name = "variant"), ModelBinder(typeof(FileModelBinder))]
            public DoujinshiVariantBase Variant { get; set; }

            [FromForm(Name = "file")]
            public IFormFile Archive { get; set; }
        }

        [HttpPost("{id}/variants")]
        public async Task<Result<UploadState>> UploadVariantAsync(Guid id, [FromForm] UploadVariantRequest request)
        {
            if (request.Archive == null)
                return Result.BadRequest("File is not selected.");

            var dbRequest = new CreateDoujinshiVariantRequest
            {
                Variant = request.Variant
            };

            using (var stream = request.Archive.OpenReadStream())
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, false))
                await _uploader.InitializeRequestAsync(archive, dbRequest);

            return await _client.CreateDoujinshiVariantAsync(id, dbRequest);
        }

        [HttpPut("{id}/variants/{variantId}")]
        public async Task<Result<DoujinshiVariant>> UpdateVariantAsync(Guid id, Guid variantId, DoujinshiVariantBase variant)
            => await _client.UpdateDoujinshiVariantAsync(id, variantId, variant);

        [HttpDelete("{id}/variants/{variantId}")]
        public async Task<Result> DeleteVariantAsync(Guid id, Guid variantId, [FromQuery] string reason)
        {
            await _client.DeleteDoujinshiVariantAsync(id, variantId, reason);

            return Result.Ok();
        }

        [HttpGet("{id}/variants/{variantId}/{index}")]
        public async Task<IActionResult> GetImageAsync(Guid id, Guid variantId, int index)
        {
            // todo: cache
            var doujinshi = await _client.GetDoujinshiAsync(id);
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