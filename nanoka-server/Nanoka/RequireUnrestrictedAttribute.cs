using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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

                var perms      = (UserPermissions) (int.TryParse(context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value, out var x) ? x : 0);
                var restricted = bool.TryParse(context.HttpContext.User.FindFirst("rest")?.Value, out var y) && y;

                if (!perms.HasFlag(UserPermissions.Administrator) && restricted)
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