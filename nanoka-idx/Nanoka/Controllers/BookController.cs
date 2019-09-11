using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Models;

namespace Nanoka.Controllers
{
    [ApiController]
    [Route("books")]
    public class BookController : AuthorizedControllerBase
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

        [HttpPut("{id}"), RequireUnrestricted]
        public async Task<Result<Book>> UpdateAsync(int id, BookBase model, [FromQuery] string reason)
        {
            try
            {
                return await _bookManager.UpdateAsync(id, model, UserId, reason);
            }
            catch (BookManagerException e)
            {
                return Result.BadRequest(e.Message);
            }
        }

        [HttpDelete("{id}"), RequireUnrestricted, RequireReputation(100)]
        public async Task<Result> DeleteAsync(int id, [FromQuery] string reason)
        {
            try
            {
                await _bookManager.DeleteAsync(id, UserId, reason);

                return Result.Ok();
            }
            catch (BookManagerException e)
            {
                return Result.BadRequest(e.Message);
            }
        }
    }
}