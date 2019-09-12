using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class User : UserBase, IHasId
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Username { get; set; }

        /// <summary>
        /// User email. This is returned by the API to moderator users only.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Hashed user password. This is only used internally and never returned by the API.
        /// </summary>
        [JsonProperty("secret")]
        public string Secret { get; set; }

        [JsonProperty("restrictions")]
        public UserRestriction[] Restrictions { get; set; }

        [JsonProperty("uploads")]
        public int UploadCount { get; set; }

        [JsonProperty("edits")]
        public int EditCount { get; set; }

        [JsonProperty("votes")]
        public int VoteCount { get; set; }

        [JsonProperty("reputation")]
        public double Reputation { get; set; }

        [JsonProperty("perms")]
        public UserPermissions Permissions { get; set; }
    }

    public class UserBase : IHasEntityType
    {
        public const string UsernameRegex = @"^(?=.{4,20}$)(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$";
        public const int UsernameMaxLength = 20;
        public const int PasswordMaxLength = 2048;

#region Meta

        [JsonIgnore]
        NanokaEntity IHasEntityType.Type => NanokaEntity.User;

#endregion
    }
}