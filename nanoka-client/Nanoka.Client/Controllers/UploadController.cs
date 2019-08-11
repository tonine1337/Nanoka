using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Core.Client;
using Nanoka.Core.Models;

namespace Nanoka.Client.Controllers
{
    [ApiController]
    [Route("uploads")]
    public class UploadController : ControllerBase
    {
        readonly IDatabaseClient _client;

        public UploadController(IDatabaseClient client)
        {
            _client = client;
        }

        [HttpGet("uploads/{id}")]
        public async Task<UploadState> GetUploadStateAsync(Guid id)
            => await _client.GetUploadStateAsync(id);
    }
}