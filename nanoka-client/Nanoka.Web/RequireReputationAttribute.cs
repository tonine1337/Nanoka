using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Nanoka.Core;
using Nanoka.Web.Database;

namespace Nanoka.Web
{
    public class RequireReputationAttribute : ActionFilterAttribute
    {
        readonly double _minimum;

        public RequireReputationAttribute(double minimum)
        {
            _minimum = minimum;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var db = context.HttpContext.RequestServices.GetService<NanokaDatabase>();

            var user = await db.GetUserAsync(Guid.Empty, context.HttpContext.RequestAborted);

            if (user == null)
            {
                context.Result = new BadRequestResult();
                return;
            }

            if (user.Reputation < _minimum)
            {
                context.Result = new InsufficientReputationResult(_minimum);
                return;
            }

            await next();
        }

        sealed class InsufficientReputationResult : ActionResult
        {
            readonly double _minimum;

            public InsufficientReputationResult(double minimum)
            {
                _minimum = minimum;
            }

            public override async Task ExecuteResultAsync(ActionContext context)
            {
                if (context.HttpContext.Response.HasStarted)
                    return;

                await Result.Forbidden($"Insufficient reputation. Required minimum: {_minimum:F}")
                            .ExecuteResultAsync(context);
            }
        }
    }
}