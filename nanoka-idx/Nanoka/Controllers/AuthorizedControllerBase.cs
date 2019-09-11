using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Models;

namespace Nanoka.Controllers
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
            _userId          = new Lazy<int>(() => HttpContext.ParseUserId());
            _userPermissions = new Lazy<UserPermissions>(() => HttpContext.ParseUserPermissions());
        }

        protected bool HasPermissions(UserPermissions required) => UserPermissions.HasFlag(required);
        protected bool HasAnyPermission(UserPermissions required) => (UserPermissions & required) != 0;
    }
}