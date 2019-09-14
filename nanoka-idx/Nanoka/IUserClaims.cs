using Nanoka.Models;

namespace Nanoka
{
    public interface IUserClaims
    {
        string Id { get; }
        UserPermissions Permissions { get; }
        double Reputation { get; }
        bool IsRestricted { get; }

        string Reason { get; }

        bool HasPermissions(UserPermissions required);
        bool HasAnyPermission(UserPermissions required);
    }
}