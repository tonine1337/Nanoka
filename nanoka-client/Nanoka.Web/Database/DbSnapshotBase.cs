using System;
using Nanoka.Core.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Web.Database
{
    public abstract class DbSnapshotBase<TSelf, TSnapshot, TValue>
        where TSelf : DbSnapshotBase<TSelf, TSnapshot, TValue>
        where TSnapshot : SnapshotBase<TValue>
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

        [Keyword(Name = "v", Index = false)]
        public string Value { get; set; }

        public TSelf Apply(TSnapshot snapshot, JsonSerializer serializer)
        {
            if (snapshot == null)
                return null;

            Id          = snapshot.Id.ToShortString();
            TargetId    = snapshot.TargetId.ToShortString();
            CommitterId = snapshot.CommitterId.ToShortString();
            Time        = snapshot.Time;

            Value = snapshot.Value == null ? Value : Serialize(serializer, snapshot.Value);

            return this as TSelf;
        }

        public TSnapshot ApplyTo(TSnapshot snapshot, JsonSerializer serializer)
        {
            snapshot.Id          = Id.ToGuid();
            snapshot.TargetId    = TargetId.ToGuid();
            snapshot.CommitterId = CommitterId.ToGuid();
            snapshot.Time        = Time;

            snapshot.Value = Value == null ? snapshot.Value : Deserialize(serializer, Value);

            return snapshot;
        }

        protected abstract string Serialize(JsonSerializer serializer, TValue value);
        protected abstract TValue Deserialize(JsonSerializer serializer, string value);
    }
}
