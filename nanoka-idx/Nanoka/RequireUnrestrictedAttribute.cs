using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Nanoka.Models;

namespace Nanoka
{
    public class RequireUnrestrictedAttribute : TypeFilterAttribute
    {
        public RequireUnrestrictedAttribute() : base(typeof(Filter)) { }

        sealed class Filter : IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (context.Result != null)
                    return;

                var claims = context.HttpContext.RequestServices.GetService<UserClaimSet>();

                if (!claims.HasPermissions(UserPermissions.Administrator) && claims.IsRestricted)
                    context.Result = new RestrictedResult();
            }

            sealed class RestrictedResult : ActionResult
            {
                public override async Task ExecuteResultAsync(ActionContext context)
                {
                    if (context.HttpContext.Response.HasStarted)
                        return;

                    await Result.Forbidden("May not perform this action while restricted.")
                                .ExecuteResultAsync(context);
                }
            }
        }
    }
}