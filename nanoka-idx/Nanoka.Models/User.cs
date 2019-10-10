using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class User : UserBase, IHasId
    {
        /// <summary>
        /// User ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// User username.
        /// </summary>
        /// <remarks>
        /// Username is unique at any given time.
        /// </remarks>
        [JsonProperty("name")]
        public string Username { get; set; }

        /// <summary>
        /// User email.
        /// </summary>
        /// <remarks>
        /// This field is only returned for moderators.
        /// </remarks>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Hashed user password.
        /// </summary>
        /// <remarks>
        /// This field is used internally by the API and will never be returned.
        /// </remarks>
        [JsonProperty("secret")]
        public string Secret { get; set; }

        /// <summary>
        /// List of all restrictions that this user has/had.
        /// </summary>
        [JsonProperty("restrictions")]
        public UserRestriction[] Restrictions { get; set; }

        /// <summary>
        /// Number of uploads by this user.
        /// </summary>
        [JsonProperty("uploads")]
        public int UploadCount { get; set; }

        /// <summary>
        /// Number of edits by this user.
        /// </summary>
        [JsonProperty("edits")]
        public int EditCount { get; set; }

        /// <summary>
        /// Number of votes by this user.
        /// </summary>
        [JsonProperty("votes")]
        public int VoteCount { get; set; }

        /// <summary>
        /// Reputation value.
        /// </summary>
        [JsonProperty("reputation")]
        public double Reputation { get; set; }

        /// <summary>
        /// Special user permissions.
        /// </summary>
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