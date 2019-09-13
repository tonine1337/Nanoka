using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Nanoka.Models;

namespace Nanoka
{
    public class UserClaimsAttribute : TypeFilterAttribute
    {
        public UserClaimsAttribute(UserPermissions permissions = UserPermissions.None,
                                   double reputation = 0,
                                   bool unrestricted = false)
            : base(typeof(Filter))
        {
            Arguments = new object[]
            {
                permissions,
                reputation,
                unrestricted
            };
        }

        sealed class Filter : IAuthorizationFilter
        {
            readonly UserPermissions[] _permissions;
            readonly double _reputation;
            readonly bool _unrestricted;

            public Filter(UserPermissions[] permissions, double reputation, bool unrestricted)
            {
                _permissions  = permissions;
                _reputation   = reputation;
                _unrestricted = unrestricted;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (context.Result != null)
                    return;

                var claims = context.HttpContext.RequestServices.GetService<UserClaimSet>();

                // allow admin to bypass checks
                if (claims.HasPermissions(UserPermissions.Administrator))
                    return;

                // restriction check
                if (_unrestricted && claims.IsRestricted)
                    context.Result = Result.Forbidden("May perform this action because you are restricted.");

                // permission check
                if (_permissions.Any(f => !claims.HasPermissions(f)))
                    context.Result = Result.Forbidden("Insufficient permissions to perform this action. " +
                                                      $"Required: {string.Join(", ", _permissions)}");

                // reputation check
                if (claims.Reputation < _reputation)
                    context.Result = Result.Forbidden("Insufficient reputation to perform this action. " +
                                                      $"Required: {_reputation:F}");
            }
        }
    }
}