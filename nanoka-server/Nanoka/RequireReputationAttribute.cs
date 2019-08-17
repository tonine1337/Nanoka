using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nanoka.Core.Models;

namespace Nanoka
{
    public class RequireReputationAttribute : TypeFilterAttribute
    {
        public RequireReputationAttribute(double minimum) : base(typeof(Filter))
        {
            Arguments = new object[]
            {
                minimum
            };
        }

        sealed class Filter : IAuthorizationFilter
        {
            readonly double _minimum;

            public Filter(double minimum)
            {
                _minimum = minimum;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (context.Result != null)
                    return;

                var perms      = (UserPermissions) (int.TryParse(context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value, out var x) ? x : 0);
                var reputation = double.TryParse(context.HttpContext.User.FindFirst("rep")?.Value, out var y) ? y : 0;

                if (!perms.HasFlag(UserPermissions.Administrator) && reputation < _minimum)
                    context.Result = new InsufficientReputationResult(_minimum);
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

                    await Result.Forbidden($"Insufficient reputation to perform this action. Required minimum: {_minimum:F}")
                                .ExecuteResultAsync(context);
                }
            }
        }
    }
}