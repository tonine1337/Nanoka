using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class User : UserBase
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("secret")]
        public Guid Secret { get; set; }

        [JsonProperty("registered")]
        public DateTime Registered { get; set; }

        [JsonProperty("scores_dj")]
        public UserScores DoujinshiScores { get; set; }

        [JsonProperty("scores_bo")]
        public UserScores BooruScores { get; set; }

        [JsonProperty("rep")]
        public double Reputation { get; set; }
    }

    public class UserBase
    {
        public const string UsernameRegex = @"^(?=.{4,20}$)(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$";

        [JsonProperty("name"), Required]
        [RegularExpression(UserBase.UsernameRegex)]
        public string Username { get; set; }

        [JsonProperty("restricted")]
        public bool IsRestricted { get; set; }

        [JsonProperty("perms")]
        public UserPermissions Permissions { get; set; }
    }
}