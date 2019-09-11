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
        {
            try
            {
                return await _bookManager.GetAsync(id);
            }
            catch (BookManagerException e)
            {
                return Result.BadRequest(e.Message);
            }
        }

        [HttpGet("{id}/history")]
        public async Task<Result<Snapshot<Book>[]>> GetSnapshotsAsync(int id)
        {
            try
            {
                return await _bookManager.GetSnapshotsAsync(id);
            }
            catch (BookManagerException e)
            {
                return Result.BadRequest(e.Message);
            }
        }

        [HttpPut("{id}"), RequireUnrestricted]
        public async Task<Result<Book>> UpdateAsync(int id, BookBase model)
        {
            try
            {
                return await _bookManager.UpdateAsync(id, model);
            }
            catch (BookManagerException e)
            {
                return Result.BadRequest(e.Message);
            }
        }

        [HttpDelete("{id}"), RequireUnrestricted, RequireReputation(100)]
        public async Task<Result> DeleteAsync(int id)
        {
            try
            {
                await _bookManager.DeleteAsync(id);

                return Result.Ok();
            }
            catch (BookManagerException e)
            {
                return Result.BadRequest(e.Message);
            }
        }
    }
}