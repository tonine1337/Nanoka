using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Models;
using Nanoka.Models.Requests;

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

        [HttpPost("{id}/history/revert"), RequireUnrestricted]
        public async Task<Result<Book>> RevertAsync(int id, RevertEntityRequest request)
            => await _bookManager.RevertAsync(id, request.SnapshotId);

        [HttpPut("{id}"), RequireUnrestricted]
        public async Task<Result<Book>> UpdateAsync(int id, BookBase model)
            => await _bookManager.UpdateAsync(id, model);

        [HttpDelete("{id}"), RequireUnrestricted, RequireReputation(100)]
        public async Task<Result> DeleteAsync(int id)
        {
            await _bookManager.DeleteAsync(id);
            return Result.Ok();
        }

        [HttpPut("{id}/vote")]
        public async Task<Vote> SetVoteAsync(int id, VoteBase model)
            => await _bookManager.VoteAsync(id, model.Type);

        [HttpDelete("{id}/vote")]
        public async Task<Result> UnsetVoteAsync(int id)
        {
            await _bookManager.VoteAsync(id, null);
            return Result.Ok();
        }
    }
}