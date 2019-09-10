using System;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    /// <summary>
    /// Represents a snapshot of an object in the database before an event.
    /// </summary>
    public class Snapshot
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("committer")]
        public int CommitterId { get; set; }

        [JsonProperty("entity")]
        public SnapshotEntity Entity { get; set; }

        [JsonProperty("entity_id")]
        public string EntityId { get; set; }

        [JsonProperty("event")]
        public SnapshotEvent Event { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}