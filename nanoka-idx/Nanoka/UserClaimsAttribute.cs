using System;
using System.Linq;
using System.Net;
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
                    context.Result = new ObjectResult("ID not provided in claims. You should be authorized.")
                    {
                        StatusCode = (int) HttpStatusCode.InternalServerError
                    };

                    return;
                }

                // allow admin to bypass checks
                if (!claims.IsAdministrator())
                {
                    // restriction check
                    if (_unrestricted && claims.IsRestricted)
                    {
                        context.Result = new ObjectResult("May not perform this action because you are restricted.")
                        {
                            StatusCode = (int) HttpStatusCode.Forbidden
                        };

                        return;
                    }

                    // permission check
                    if (_permissions.Any(f => !claims.HasPermissions(f)))
                    {
                        context.Result = new ObjectResult($"Insufficient permissions to perform this action. Required: {string.Join(", ", _permissions)}")
                        {
                            StatusCode = (int) HttpStatusCode.Forbidden
                        };

                        return;
                    }

                    // reputation check
                    if (claims.Reputation < _reputation)
                    {
                        context.Result = new ObjectResult($"Insufficient reputation to perform this action. Required: {_reputation:F}")
                        {
                            StatusCode = (int) HttpStatusCode.Forbidden
                        };

                        return;
                    }
                }

                // require reason for potentially damaging actions
                var reason = claims.GetReason();

                if (_reason && (string.IsNullOrEmpty(reason) || reason.Length <= 3))
                {
                    context.Result = new ObjectResult("Valid reason must be provided for this action.")
                    {
                        StatusCode = (int) HttpStatusCode.BadRequest
                    };

                    return;
                }
            }
        }
    }
}