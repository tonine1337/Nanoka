using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class UserRestriction
    {
        /// <summary>
        /// Time when this restriction started.
        /// </summary>
        [JsonProperty("start")]
        public DateTime Start { get; set; }

        /// <summary>
        /// Time when this restriction ended.
        /// </summary>
        [JsonProperty("end")]
        public DateTime End { get; set; }

        /// <summary>
        /// ID of the user that created this restriction.
        /// </summary>
        [JsonProperty("moderator")]
        public string ModeratorId { get; set; }

        /// <summary>
        /// Reason describing why this restriction was created.
        /// </summary>
        [JsonProperty("reason"), Required, MinLength(5), MaxLength(2048)]
        public string Reason { get; set; }
    }
}