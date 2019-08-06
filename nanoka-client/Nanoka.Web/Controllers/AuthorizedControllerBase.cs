using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Core.Models;

namespace Nanoka.Web.Controllers
{
    [Authorize]
    public abstract class AuthorizedControllerBase : ControllerBase
    {
        readonly Lazy<int> _userId;
        readonly Lazy<UserPermissions> _userPermissions;

        protected int UserId => _userId.Value;
        protected UserPermissions UserPermissions => _userPermissions.Value;

        protected AuthorizedControllerBase()
        {
            _userId = new Lazy<int>(
                () => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0);

            _userPermissions = new Lazy<UserPermissions>(
                () => (UserPermissions) (int.TryParse(User.FindFirst(ClaimTypes.Role)?.Value, out var x) ? x : 0));
        }

        protected bool HasPermissions(UserPermissions required) => UserPermissions.HasFlag(required);
        protected bool HasAnyPermission(UserPermissions required) => (UserPermissions & required) != 0;
    }
}
