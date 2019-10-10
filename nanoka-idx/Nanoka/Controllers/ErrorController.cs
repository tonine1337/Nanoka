using System.Net;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Models;

namespace Nanoka.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [Route("error")]
        public Result<object> Handle()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

            var builder = new StringBuilder()
               .Append("An internal server error caused this request to fail.");

            if (exception != null)
                builder.AppendLine()
                       .Append("Message: ")
                       .AppendLine(exception.Message ?? "<null>")
                       .Append("Trace: ")
                       .Append(exception.StackTrace.Substring(0, exception.StackTrace.IndexOf('\n')).Trim());

            return new Result<object>(HttpStatusCode.InternalServerError, builder.ToString(), null);
        }

        [Route("error/{status}")]
        public Result<object> Handle(HttpStatusCode status)
            => new Result<object>(status, status.ToString(), null);
    }
}