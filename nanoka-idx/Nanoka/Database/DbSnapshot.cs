using System;
using Nanoka.Models;
using Nest;
using Newtonsoft.Json;
using Snapshot = Nanoka.Models.Snapshot;

namespace Nanoka.Database
{
    [ElasticsearchType(IdProperty = nameof(Id), RelationName = nameof(Snapshot))]
    public class DbSnapshot
    {
        [Keyword(Name = "id", Index = false), JsonProperty("id")]
        public int Id { get; set; }

        [Keyword(Name = "b", Index = false), JsonProperty("b")]
        public int? RollbackId { get; set; }

        [Date(Name = "t"), JsonProperty("t")]
        public DateTime Time { get; set; }

        [Keyword(Name = "c"), JsonProperty("c")]
        public int CommitterId { get; set; }

        [Keyword(Name = "s"), JsonProperty("s")]
        public SnapshotType Type { get; set; }

        [Keyword(Name = "e"), JsonProperty("e")]
        public NanokaEntity EntityType { get; set; }

        [Keyword(Name = "x"), JsonProperty("x")]
        public int EntityId { get; set; }

        [Keyword(Name = "a"), JsonProperty("a")]
        public SnapshotEvent Event { get; set; }

        [Text(Name = "r"), JsonProperty("r")]
        public string Reason { get; set; }

        [Keyword(Name = "v", Index = false), JsonProperty("v")]
        public string ValueSerialized { get; set; }

        public Snapshot<T> ToSnapshot<T>(JsonSerializer serializer) => new Snapshot<T>
        {
            Id          = Id,
            RollbackId  = RollbackId,
            Time        = Time,
            CommitterId = CommitterId,
            Type        = Type,
            EntityType  = EntityType,
            EntityId    = EntityId,
            Event       = Event,
            Reason      = Reason,
            Value       = serializer.Deserialize<T>(ValueSerialized)
        };

        public static DbSnapshot FromSnapshot<T>(Snapshot<T> snapshot, JsonSerializer serializer) => new DbSnapshot
        {
            Id              = snapshot.Id,
            RollbackId      = snapshot.RollbackId,
            Time            = snapshot.Time,
            CommitterId     = snapshot.CommitterId,
            Type            = snapshot.Type,
            EntityType      = snapshot.EntityType,
            EntityId        = snapshot.EntityId,
            Event           = snapshot.Event,
            Reason          = snapshot.Reason,
            ValueSerialized = serializer.Serialize(snapshot.Value)
        };
    }
}