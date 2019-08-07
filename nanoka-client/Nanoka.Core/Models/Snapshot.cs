using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class Snapshot<T>
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("tid")]
        public Guid TargetId { get; set; }

        [JsonProperty("committer")]
        public Guid CommitterId { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("event")]
        public SnapshotEvent Event { get; set; }

        [JsonProperty("target")]
        public SnapshotTarget Target { get; set; }

        [JsonProperty("value"), Required]
        public T Value { get; set; }
    }
}