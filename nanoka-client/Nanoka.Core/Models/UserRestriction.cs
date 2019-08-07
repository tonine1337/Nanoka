using System;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class UserRestriction
    {
        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("end")]
        public DateTime End { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        /// <summary>
        /// ID of the user that effectuated this restriction.
        /// </summary>
        [JsonProperty("source")]
        public Guid Source { get; set; }
    }
}