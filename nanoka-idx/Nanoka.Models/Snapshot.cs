using System;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    /// <summary>
    /// Use <see cref="Snapshot{T}"/> instead.
    /// </summary>
    public abstract class Snapshot : Snapshot<object> { }

    /// <summary>
    /// Represents a snapshot of an object in the database before an event.
    /// </summary>
    public class Snapshot<T> : IHasId
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("committer")]
        public int CommitterId { get; set; }

        [JsonProperty("type")]
        public SnapshotType Type { get; set; }

        [JsonProperty("entity")]
        public NanokaEntity EntityType { get; set; }

        [JsonProperty("entity_id")]
        public int EntityId { get; set; }

        [JsonProperty("event")]
        public SnapshotEvent Event { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("value")]
        public T Value { get; set; }
    }
}