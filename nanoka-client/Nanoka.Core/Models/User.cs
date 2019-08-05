using System;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class User
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("secret")]
        public Guid Secret { get; set; }

        [JsonProperty("name")]
        public string Username { get; set; }

        [JsonProperty("restricted")]
        public bool IsRestricted { get; set; }

        [JsonProperty("perms")]
        public UserPermissions Permissions { get; set; }

        [JsonProperty("scores_dj")]
        public UserScores DoujinshiScores { get; set; }

        [JsonProperty("scores_bo")]
        public UserScores BooruScores { get; set; }

        [JsonProperty("rep")]
        public double Reputation { get; set; }
    }
}