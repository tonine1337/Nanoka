using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public abstract class Snapshot<T> : SnapshotBase<T>
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("tid")]
        public Guid TargetId { get; set; }

        [JsonProperty("approver")]
        public Guid? ApproverId { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }
    }

    public abstract class SnapshotBase<T>
    {
        [JsonProperty("reason"), Required]
        public string Reason { get; set; }

        [JsonProperty("value"), Required]
        public T Value { get; set; }
    }
}