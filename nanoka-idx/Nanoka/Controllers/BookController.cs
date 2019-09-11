using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Models;

namespace Nanoka.Controllers
{
    [ApiController]
    [Route("books")]
    [Authorize]
    public class BookController : ControllerBase
    {
        readonly BookManager _bookManager;

        public BookController(BookManager bookManager)
        {
            _bookManager = bookManager;
        }

        [HttpGet("{id}")]
        public async Task<Result<Book>> GetAsync(int id)
            => await _bookManager.GetAsync(id);

        [HttpGet("{id}/history")]
        public async Task<Result<Snapshot<Book>[]>> GetSnapshotsAsync(int id)
            => await _bookManager.GetSnapshotsAsync(id);

        [HttpPut("{id}"), RequireUnrestricted]
        public async Task<Result<Book>> UpdateAsync(int id, BookBase model)
            => await _bookManager.UpdateAsync(id, model);

        [HttpDelete("{id}"), RequireUnrestricted, RequireReputation(100)]
        public async Task<Result> DeleteAsync(int id)
        {
            await _bookManager.DeleteAsync(id);

            return Result.Ok();
        }
    }
}