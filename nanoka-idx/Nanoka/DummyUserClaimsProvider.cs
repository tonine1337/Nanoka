using System.Collections.Generic;
using System.Linq;
using Nanoka.Models;

namespace Nanoka
{
    public class DummyUserClaimsProvider : IUserClaims
    {
        public string Id { get; set; }
        public UserPermissions Permissions { get; set; }
        public double Reputation { get; set; }
        public bool IsRestricted { get; set; }

        IReadOnlyDictionary<string, string> _queryParams = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> QueryParams
        {
            get => _queryParams;
            set => _queryParams = value.ToDictionary(x => x.Key.ToLowerInvariant(), x => x.Value);
        }
    }
}
