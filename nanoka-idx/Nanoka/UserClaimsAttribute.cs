using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Nanoka.Models;

namespace Nanoka
{
    public class UserClaimsAttribute : TypeFilterAttribute
    {
        static readonly UserPermissions[] _permFlags = Enum.GetValues(typeof(UserPermissions))
                                                           .Cast<UserPermissions>()
                                                           .ToArray();

        public UserClaimsAttribute(UserPermissions permissions = UserPermissions.None,
                                   double reputation = 0,
                                   bool unrestricted = false,
                                   bool reason = false)
            : base(typeof(Filter))
        {
            Arguments = new object[]
            {
                _permFlags.Where(f => permissions.HasFlag(f)).ToArray(),
                reputation,
                unrestricted,
                reason
            };
        }

        sealed class Filter : IAuthorizationFilter
        {
            readonly UserPermissions[] _permissions;
            readonly double _reputation;
            readonly bool _unrestricted;
            readonly bool _reason;

            public Filter(UserPermissions[] permissions,
                          double reputation,
                          bool unrestricted,
                          bool reason)
            {
                _permissions  = permissions;
                _reputation   = reputation;
                _unrestricted = unrestricted;
                _reason       = reason;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (context.Result != null)
                    return;

                var claims = context.HttpContext.RequestServices.GetService<IUserClaims>();

                if (string.IsNullOrEmpty(claims.Id))
                {
                    context.Result = Result.InternalServerError("ID not provided in claims. You should be authorized.");
                    return;
                }

                // allow admin to bypass checks
                if (!claims.IsAdministrator())
                {
                    // restriction check
                    if (_unrestricted && claims.IsRestricted)
                    {
                        context.Result = Result.Forbidden("May perform this action because you are restricted.");
                        return;
                    }

                    // permission check
                    if (_permissions.Any(f => !claims.HasPermissions(f)))
                    {
                        context.Result = Result.Forbidden($"Insufficient permissions to perform this action. Required: {string.Join(", ", _permissions)}");
                        return;
                    }

                    // reputation check
                    if (claims.Reputation < _reputation)
                    {
                        context.Result = Result.Forbidden($"Insufficient reputation to perform this action. Required: {_reputation:F}");
                        return;
                    }
                }

                // require reason for potentially damaging actions
                if (_reason && (string.IsNullOrEmpty(claims.Reason) || claims.Reason.Length <= 3))
                {
                    context.Result = Result.BadRequest("Valid reason must be provided for this action.");
                    return;
                }
            }
        }
    }
}