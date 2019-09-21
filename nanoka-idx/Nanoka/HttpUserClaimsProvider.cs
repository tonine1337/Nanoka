using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Nanoka.Models;

namespace Nanoka
{
    public class HttpUserClaimsProvider : IUserClaims
    {
        public string Id { get; }
        public UserPermissions Permissions { get; }
        public double Reputation { get; }
        public bool IsRestricted { get; }

        public IReadOnlyDictionary<string, string> QueryParams { get; }

        public HttpUserClaimsProvider(IHttpContextAccessor httpContextAccessor)
        {
            var context = httpContextAccessor.HttpContext;

            Id           = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Permissions  = int.TryParse(context.User.FindFirst(ClaimTypes.Role)?.Value, out var b) ? (UserPermissions) b : UserPermissions.None;
            Reputation   = double.TryParse(context.User.FindFirst("rep")?.Value, out var c) ? c : 0;
            IsRestricted = bool.TryParse(context.User.FindFirst("rest")?.Value, out var d) && d;

            var query = new Dictionary<string, string>();

            foreach (var (k, v) in context.Request.Query)
                query[k.ToLowerInvariant()] = ((string) v).Trim();

            QueryParams = query;
        }
    }
}