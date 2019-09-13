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

        [HttpPut("{id}"), UserClaims(unrestricted: true)]
        public async Task<Result<Book>> UpdateAsync(int id, BookBase model)
            => await _bookManager.UpdateAsync(id, model);

        [HttpDelete("{id}"), UserClaims(unrestricted: true, reputation: 100), VerifyHuman]
        public async Task<Result> DeleteAsync(int id)
        {
            await _bookManager.DeleteAsync(id);
            return Result.Ok();
        }

        [HttpGet("{id}/snapshots")]
        public async Task<Result<Snapshot<Book>[]>> GetSnapshotsAsync(int id)
            => await _bookManager.GetSnapshotsAsync(id);

        [HttpPost("{id}/snapshots/revert"), UserClaims(unrestricted: true)]
        public async Task<Result<Book>> RevertAsync(int id, RevertEntityRequest request)
            => await _bookManager.RevertAsync(id, request.SnapshotId);

        [HttpPut("{id}/vote")]
        public async Task<Vote> SetVoteAsync(int id, VoteBase model)
            => await _bookManager.VoteAsync(id, model.Type);

        [HttpDelete("{id}/vote")]
        public async Task<Result> UnsetVoteAsync(int id)
        {
            await _bookManager.VoteAsync(id, null);
            return Result.Ok();
        }

        [HttpGet("{id}/contents/{contentId}")]
        public async Task<Result<BookContent>> GetContentAsync(int id, int contentId)
            => (await _bookManager.GetContentAsync(id, contentId)).content;

        [HttpPut("{id}/contents/{contentId}")]
        public async Task<Result<BookContent>> UpdateContentAsync(int id, int contentId, BookContentBase model)
            => await _bookManager.UpdateContentAsync(id, contentId, model);

        [HttpDelete("{id}/contents/{contentId}"), UserClaims(unrestricted: true, reputation: 100), VerifyHuman]
        public async Task<Result> DeleteContentAsync(int id, int contentId)
        {
            await _bookManager.RemoveContentAsync(id, contentId);
            return Result.Ok();
        }
    }
}