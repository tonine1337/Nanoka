using System;
using Nanoka.Core.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Web.Database
{
    public abstract class DbSnapshotBase<TSelf, TSnapshot, TValue>
        where TSelf : DbSnapshotBase<TSelf, TSnapshot, TValue>
        where TSnapshot : Snapshot<TValue>
        where TValue : new()
    {
        [Keyword]
        public string Id { get; set; }

        [Keyword(Name = "t")]
        public string TargetId { get; set; }

        [Keyword(Name = "a")]
        public string ApproverId { get; set; }

        [Date(Name = "tm")]
        public DateTime Time { get; set; }

        [Text(Name = "r")]
        public string Reason { get; set; }

        [Keyword(Name = "v", Index = false)]
        public string Value { get; set; }

        public TSelf Apply(TSnapshot snapshot, JsonSerializer serializer)
        {
            if (snapshot == null)
                return null;

            Id         = snapshot.Id.ToShortString();
            TargetId   = snapshot.TargetId.ToShortString();
            ApproverId = snapshot.ApproverId?.ToShortString(); // ?? ApproverId // null does not indicate unspecified
            Time       = snapshot.Time;
            Reason     = snapshot.Reason ?? Reason;

            Value = snapshot.Value == null ? Value : Serialize(serializer, snapshot.Value);

            return this as TSelf;
        }

        public TSnapshot ApplyTo(TSnapshot snapshot, JsonSerializer serializer)
        {
            snapshot.Id         = Id.ToGuid();
            snapshot.TargetId   = TargetId.ToGuid();
            snapshot.ApproverId = ApproverId?.ToGuid();
            snapshot.Time       = Time;
            snapshot.Reason     = Reason ?? snapshot.Reason;

            snapshot.Value = Value == null ? snapshot.Value : Deserialize(serializer, Value);

            return snapshot;
        }

        protected abstract string Serialize(JsonSerializer serializer, TValue value);
        protected abstract TValue Deserialize(JsonSerializer serializer, string value);
    }
}