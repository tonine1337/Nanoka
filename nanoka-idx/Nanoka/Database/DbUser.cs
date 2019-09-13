using Nanoka.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Database
{
    [ElasticsearchType(RelationName = nameof(User))]
    public class DbUser : IHasId
    {
        [Keyword(Name = "id", Index = false), JsonProperty("id")]
        public string Id { get; set; }

        [Keyword(Name = "n"), JsonProperty("n")]
        public string Username { get; set; }

        [Keyword(Name = "m"), JsonProperty("m")]
        public string Email { get; set; }

        [Keyword(Name = "s", Index = false), JsonProperty("s")]
        public string Secret { get; set; }

        [Nested(Name = "r"), JsonProperty("r")]
        public DbUserRestriction[] Restrictions { get; set; }

        [Number(NumberType.Integer, Name = "cu"), JsonProperty("cu")]
        public int UploadCount { get; set; }

        [Number(NumberType.Integer, Name = "ce"), JsonProperty("ce")]
        public int EditCount { get; set; }

        [Number(NumberType.Integer, Name = "cv"), JsonProperty("cv")]
        public int VoteCount { get; set; }

        [Number(NumberType.Double, Name = "rep"), JsonProperty("rep")]
        public double Reputation { get; set; }

        [Keyword(Name = "p", Index = false), JsonProperty("p")]
        public UserPermissions Permissions { get; set; }

        public User ToUser() => new User
        {
            Id           = Id,
            Username     = Username,
            Email        = Email,
            Secret       = Secret,
            Restrictions = Restrictions?.ToArray(r => r.ToRestriction()),
            UploadCount  = UploadCount,
            EditCount    = EditCount,
            VoteCount    = VoteCount,
            Reputation   = Reputation,
            Permissions  = Permissions
        };

        public static DbUser FromUser(User user) => new DbUser
        {
            Id           = user.Id,
            Username     = user.Username,
            Email        = user.Email,
            Secret       = user.Secret,
            Restrictions = user.Restrictions?.ToArray(DbUserRestriction.FromRestriction),
            UploadCount  = user.UploadCount,
            EditCount    = user.EditCount,
            VoteCount    = user.VoteCount,
            Reputation   = user.Reputation,
            Permissions  = user.Permissions
        };
    }
}
