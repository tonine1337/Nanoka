using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Nanoka.Web.Controllers
{
    [ApiController]
    [Route("error")]
    public class ErrorController : ControllerBase
    {
        public Result Handle()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>().Error;

            return Result.InternalServerError("An internal server error caused this request to fail. " +
                                              $"{exception.Message ?? "<unknown reason>"}");
        }
    }
}