using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Core.Client;
using Nanoka.Core.Models;

namespace Nanoka.Core.Controllers
{
    [ApiController]
    [Route("doujinshi")]
    public class DoujinshiController : ControllerBase
    {
        readonly IDatabaseClient _client;

        public DoujinshiController(IDatabaseClient client)
        {
            _client = client;
        }

        [HttpGet("{id}")]
        public async Task<Result<Doujinshi>> GetAsync(Guid id) => await _client.GetDoujinshiAsync(id);
    }
}
