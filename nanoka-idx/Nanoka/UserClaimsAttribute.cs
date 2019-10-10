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
        public const int MinReasonLength = 4;

        static readonly UserPermissions[] _permFlags = Enum.GetValues(typeof(UserPermissions))
                                                           .Cast<UserPermissions>()
                                                           .Where(p => p != UserPermissions.None)
                                                           .ToArray();

        public UserPermissions[] PermissionFlags { get; private set; } = new UserPermissions[0];

        double _reputation;
        bool _unrestricted;
        bool _reason;

        public UserPermissions Permissions
        {
            get => PermissionFlags.Aggregate(UserPermissions.None, (x, y) => x | y);
            set
            {
                PermissionFlags = _permFlags.Where(f => value.HasFlag(f)).ToArray();
                SetArguments();
            }
        }

        public double Reputation
        {
            get => _reputation;
            set
            {
                _reputation = value;
                SetArguments();
            }
        }

        public bool Unrestricted
        {
            get => _unrestricted;
            set
            {
                _unrestricted = value;
                SetArguments();
            }
        }

        public bool Reason
        {
            get => _reason;
            set
            {
                _reason = value;
                SetArguments();
            }
        }

        public UserClaimsAttribute() : base(typeof(Filter)) { }

        void SetArguments() => Arguments = new object[]
        {
            PermissionFlags,
            _reputation,
            _unrestricted,
            _reason
        };

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

                if (_reason && (string.IsNullOrEmpty(reason) || reason.Length < MinReasonLength))
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