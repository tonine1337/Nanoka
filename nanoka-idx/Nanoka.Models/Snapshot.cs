using System;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    /// <summary>
    /// Use <see cref="Snapshot{T}"/> instead.
    /// </summary>
    public abstract class Snapshot : Snapshot<object> { }

    /// <summary>
    /// Represents a snapshot of an object in the database after an event.
    /// </summary>
    public class Snapshot<T> : IHasId
    {
        /// <summary>
        /// Snapshot ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// If <see cref="Event"/> is <see cref="SnapshotEvent.Rollback"/>,  the ID of the snapshot that was reverted to.
        /// Otherwise, this field should be null.
        /// </summary>
        [JsonProperty("rollbackId")]
        public string RollbackId { get; set; }

        /// <summary>
        /// Time when this snapshot was taken.
        /// </summary>
        [JsonProperty("time")]
        public DateTime Time { get; set; }

        /// <summary>
        /// ID of the user who created this snapshot.
        /// </summary>
        [JsonProperty("committer")]
        public string CommitterId { get; set; }

        /// <summary>
        /// Type of this snapshot.
        /// </summary>
        [JsonProperty("type")]
        public SnapshotType Type { get; set; }

        /// <summary>
        /// Type of the entity that this snapshot represents.
        /// </summary>
        [JsonProperty("entity")]
        public NanokaEntity EntityType { get; set; }

        /// <summary>
        /// ID of the entity that this snapshot represents.
        /// </summary>
        /// <remarks>
        /// It is possible for the entity to not exist if it was deleted after this snapshot was taken.
        /// </remarks>
        [JsonProperty("entityId")]
        public string EntityId { get; set; }

        /// <summary>
        /// Snapshot event.
        /// </summary>
        [JsonProperty("event")]
        public SnapshotEvent Event { get; set; }

        /// <summary>
        /// Reason describing why this snapshot was taken.
        /// </summary>
        [JsonProperty("reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Snapshot value.
        /// </summary>
        [JsonProperty("value")]
        public T Value { get; set; }

        public override string ToString() => $"{EntityType} {EntityId} [{Type}]: \"{Reason ?? "<no reason>"}\" #{Id}";
    }
}