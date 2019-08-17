using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Core;
using Nanoka.Core.Models;

namespace Nanoka.Web.Controllers
{
    [Authorize]
    public abstract class AuthorizedControllerBase : ControllerBase
    {
        readonly Lazy<Guid> _userId;
        readonly Lazy<UserPermissions> _userPermissions;

        protected Guid UserId => _userId.Value;
        protected UserPermissions UserPermissions => _userPermissions.Value;

        protected AuthorizedControllerBase()
        {
            _userId = new Lazy<Guid>(
                () =>
                {
                    var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (value == null)
                        throw new InvalidOperationException($"Missing claim '{nameof(ClaimTypes.NameIdentifier)}'.");

                    return value.ToGuid();
                });

            _userPermissions = new Lazy<UserPermissions>(
                () =>
                {
                    var value = User.FindFirst(ClaimTypes.Role)?.Value;

                    if (value == null)
                        throw new InvalidOperationException($"Missing claim '{nameof(ClaimTypes.Role)}'.");

                    if (int.TryParse(value, out var x))
                        return (UserPermissions) x;

                    throw new NotSupportedException($"Invalid permission value '{value}'.");
                });
        }

        protected bool HasPermissions(UserPermissions required) => UserPermissions.HasFlag(required);
        protected bool HasAnyPermission(UserPermissions required) => (UserPermissions & required) != 0;
    }
}