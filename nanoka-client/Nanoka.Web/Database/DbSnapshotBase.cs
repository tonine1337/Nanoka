using System;
using Nanoka.Core.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Web.Database
{
    public abstract class DbSnapshotBase<TSelf, TValue>
        where TSelf : DbSnapshotBase<TSelf, TValue>
        where TValue : new()
    {
        [Keyword]
        public string Id { get; set; }

        [Keyword(Name = "t")]
        public string TargetId { get; set; }

        [Keyword(Name = "c")]
        public string CommitterId { get; set; }

        [Date(Name = "tm")]
        public DateTime Time { get; set; }

        [Number(Name = "e")]
        public SnapshotEvent Event { get; set; }

        [Keyword(Name = "v", Index = false)]
        public string Value { get; set; }

        public TSelf Apply(Snapshot<TValue> snapshot, JsonSerializer serializer)
        {
            if (snapshot == null)
                return null;

            Id          = snapshot.Id.ToShortString();
            TargetId    = snapshot.TargetId.ToShortString();
            CommitterId = snapshot.CommitterId.ToShortString();
            Time        = snapshot.Time;
            Event       = snapshot.Event;

            Value = snapshot.Value == null ? Value : Serialize(serializer, snapshot.Value);

            return this as TSelf;
        }

        public Snapshot<TValue> ApplyTo(Snapshot<TValue> snapshot, JsonSerializer serializer)
        {
            snapshot.Id          = Id.ToGuid();
            snapshot.TargetId    = TargetId.ToGuid();
            snapshot.CommitterId = CommitterId.ToGuid();
            snapshot.Time        = Time;
            snapshot.Event       = Event;

            snapshot.Value = Value == null ? snapshot.Value : Deserialize(serializer, Value);

            return snapshot;
        }

        protected abstract string Serialize(JsonSerializer serializer, TValue value);
        protected abstract TValue Deserialize(JsonSerializer serializer, string value);
    }
}