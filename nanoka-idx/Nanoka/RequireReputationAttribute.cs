using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Nanoka.Models;

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

                var claims = context.HttpContext.RequestServices.GetService<UserClaimSet>();

                if (!claims.HasPermissions(UserPermissions.Administrator) && claims.Reputation < _minimum)
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