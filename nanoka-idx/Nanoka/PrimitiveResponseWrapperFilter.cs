using System.Net;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nanoka.Models;

namespace Nanoka
{
    public class PrimitiveResponseWrapperFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            switch (context.Result)
            {
                case StatusCodeResult result:
                {
                    var status = (HttpStatusCode) result.StatusCode;

                    context.Result = new ObjectResult(new ResultModel<object>(status, status.ToString(), null))
                    {
                        StatusCode = (int) status
                    };

                    break;
                }

                case ObjectResult result:
                {
                    var status = (HttpStatusCode) (result.StatusCode ?? 200);

                    var type = result.Value?.GetType();

                    // ensure something is returned
                    if (type == null)
                    {
                        context.Result = new ObjectResult(new ResultModel<object>(status, status.ToString(), null))
                        {
                            StatusCode = (int) status
                        };
                    }

                    // use string results as message
                    else if (type == typeof(string))
                    {
                        var message = (string) result.Value;

                        context.Result = new ObjectResult(new ResultModel<object>(status, message, null))
                        {
                            StatusCode = (int) status
                        };

                        context.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = message;
                    }

                    // wrap primitive type responses
                    else if (type.IsPrimitive || type.IsValueType)
                    {
                        context.Result = new ObjectResult(new ResultModel<object>(status, status.ToString(), result.Value))
                        {
                            StatusCode = (int) status
                        };
                    }

                    break;
                }
            }
        }

        public void OnResultExecuted(ResultExecutedContext context) { }
    }
}