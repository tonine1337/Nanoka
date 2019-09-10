using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class UserRestriction
    {
        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("end")]
        public DateTime End { get; set; }

        [JsonProperty("moderator")]
        public string ModeratorId { get; set; }
    }

    public class UserRestrictionBase
    {
        [JsonProperty("reason"), Required, MinLength(5)]
        public string Reason { get; set; }
    }
}