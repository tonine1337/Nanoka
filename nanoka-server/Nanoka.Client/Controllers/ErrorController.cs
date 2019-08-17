using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Nanoka.Client.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [Route("error")]
        public string Handle()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

            return exception?.ToString() ?? "Unknown error.";
        }
    }
}