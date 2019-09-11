using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Nanoka.Models;

namespace Nanoka
{
    public class UserClaimSet
    {
        public int Id { get; }
        public UserPermissions Permissions { get; }
        public double Reputation { get; }
        public bool IsRestricted { get; }

        public string Reason { get; }

        public UserClaimSet(IHttpContextAccessor httpContextAccessor)
        {
            var ctx = httpContextAccessor.HttpContext;

            Id           = int.TryParse(ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var a) ? a : 0;
            Permissions  = int.TryParse(ctx.User.FindFirst(ClaimTypes.Role)?.Value, out var b) ? (UserPermissions) b : UserPermissions.None;
            Reputation   = double.TryParse(ctx.User.FindFirst("rep")?.Value, out var c) ? c : 0;
            IsRestricted = bool.TryParse(ctx.User.FindFirst("rest")?.Value, out var d) && d;

            Reason = ctx.Request.Query["reason"];
        }

        public bool HasPermissions(UserPermissions required) => Permissions.HasFlag(required);
        public bool HasAnyPermission(UserPermissions required) => (Permissions & required) != 0;
    }
}