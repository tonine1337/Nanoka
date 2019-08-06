using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class BooruPostSnapshot : BooruPostSnapshotBase
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("tid")]
        public Guid TargetId { get; set; }

        [JsonProperty("approver")]
        public Guid? ApproverId { get; set; }
    }

    public class BooruPostSnapshotBase
    {
        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("reason"), Required]
        public string Reason { get; set; }

        [JsonProperty("value"), Required]
        public BooruPost Value { get; set; }
    }
}