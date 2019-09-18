using Nanoka.Models;

namespace Nanoka
{
    public class DummyUserClaimsProvider : IUserClaims
    {
        public string Id { get; set; }
        public UserPermissions Permissions { get; set; }
        public double Reputation { get; set; }
        public bool IsRestricted { get; set; }
        public string Reason { get; set; }
    }
}