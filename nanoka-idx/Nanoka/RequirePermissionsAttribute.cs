using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Nanoka.Models;

namespace Nanoka
{
    public class RequirePermissionsAttribute : TypeFilterAttribute
    {
        static readonly UserPermissions[] _allFlags = Enum.GetValues(typeof(UserPermissions))
                                                          .Cast<UserPermissions>()
                                                          .ToArray();

        public RequirePermissionsAttribute(UserPermissions required) : base(typeof(Filter))
        {
            Arguments = new object[]
            {
                _allFlags.Where(f => required.HasFlag(f)).ToArray()
            };
        }

        sealed class Filter : IAuthorizationFilter
        {
            readonly UserPermissions[] _requiredFlags;

            public Filter(UserPermissions[] requiredFlags)
            {
                _requiredFlags = requiredFlags;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (context.Result != null)
                    return;

                var claims = context.HttpContext.RequestServices.GetService<UserClaimSet>();

                if (!claims.HasPermissions(UserPermissions.Administrator) && _requiredFlags.Any(f => !claims.HasPermissions(f)))
                    context.Result = new InsufficientPermissionsResult(_requiredFlags);
            }

            sealed class InsufficientPermissionsResult : ActionResult
            {
                readonly UserPermissions[] _requiredFlags;

                public InsufficientPermissionsResult(UserPermissions[] requiredFlags)
                {
                    _requiredFlags = requiredFlags;
                }

                public override async Task ExecuteResultAsync(ActionContext context)
                {
                    if (context.HttpContext.Response.HasStarted)
                        return;

                    await Result.Forbidden(_requiredFlags)
                                .ExecuteResultAsync(context);
                }
            }
        }
    }
}