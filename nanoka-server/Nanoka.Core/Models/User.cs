using System;
using System.Collections.Generic;
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

        [JsonProperty("restrictions")]
        public List<UserRestriction> Restrictions { get; set; }

        [JsonProperty("upload")]
        public int UploadCount { get; set; }

        [JsonProperty("edit")]
        public int EditCount { get; set; }

        [JsonProperty("upvoted")]
        public int UpvotedCount { get; set; }

        [JsonProperty("downvoted")]
        public int DownvotedCount { get; set; }

        [JsonProperty("rep")]
        public double Reputation { get; set; }
    }

    public class UserBase
    {
        public const string UsernameRegex = @"^(?=.{4,20}$)(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$";

        [JsonProperty("name"), Required]
        [RegularExpression(UsernameRegex)]
        public string Username { get; set; }

        [JsonProperty("perms")]
        public UserPermissions Permissions { get; set; }
    }
}