using System.Collections.Generic;
using Nanoka.Models;

namespace Nanoka
{
    public interface IUserClaims
    {
        string Id { get; }
        UserPermissions Permissions { get; }
        int Version { get; }
        double Reputation { get; }
        bool IsRestricted { get; }

        IReadOnlyDictionary<string, string> QueryParams { get; }
    }

    public static class UserClaimsExtensions
    {
        public static bool IsAdministrator(this IUserClaims claims)
            => claims.Permissions.HasFlag(UserPermissions.Administrator);

        public static bool HasPermissions(this IUserClaims claims, UserPermissions required)
            => claims.IsAdministrator() || claims.Permissions.HasFlag(required);

        public static bool HasAnyPermission(this IUserClaims claims, UserPermissions requiredAny)
            => claims.IsAdministrator() || (claims.Permissions & requiredAny) != 0;

        public static string GetReason(this IUserClaims claims) => claims.QueryParams.GetValueOrDefault("reason");

        public static (int start, int end)? GetRange(this IUserClaims claims)
        {
            if (int.TryParse(claims.QueryParams.GetValueOrDefault("start"), out var start) &&
                int.TryParse(claims.QueryParams.GetValueOrDefault("end"), out var end))
                return (start, end);

            return null;
        }
    }
}