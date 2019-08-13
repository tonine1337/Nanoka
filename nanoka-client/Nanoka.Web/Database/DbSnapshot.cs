using System;
using Nanoka.Core;
using Nanoka.Core.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Web.Database
{
    [ElasticsearchType(RelationName = nameof(Snapshot), IdProperty = nameof(Id))]
    public class DbSnapshot
    {
        [Keyword(Name = "id", Index = false)]
        public string Id { get; set; }

        [Keyword(Name = "t")]
        public string TargetId { get; set; }

        [Keyword(Name = "c")]
        public string CommitterId { get; set; }

        [Date(Name = "tm")]
        public DateTime Time { get; set; }

        [Number(Name = "e")]
        public SnapshotEvent Event { get; set; }

        [Text(Name = "r")]
        public string Reason { get; set; }

        [Keyword(Name = "v", Index = false)]
        public string Value { get; set; }

        public DbSnapshot Apply(Snapshot<Doujinshi> snapshot, JsonSerializer serializer)
            => ApplyInternal(snapshot, serializer, new DbDoujinshi().Apply);

        public DbSnapshot Apply(Snapshot<BooruPost> snapshot, JsonSerializer serializer)
            => ApplyInternal(snapshot, serializer, new DbBooruPost().Apply);

        DbSnapshot ApplyInternal<T>(Snapshot<T> snapshot,
                                    JsonSerializer serializer,
                                    Func<T, object> project)
        {
            if (snapshot == null)
                return null;

            Id          = snapshot.Id.ToShortString();
            TargetId    = snapshot.TargetId.ToShortString();
            CommitterId = snapshot.CommitterId.ToShortString();
            Time        = snapshot.Time;
            Event       = snapshot.Event;
            Reason      = snapshot.Reason ?? Reason;

            Value = snapshot.Value == null ? Value : serializer.Serialize(project(snapshot.Value));

            return this;
        }

        public Snapshot<Doujinshi> ApplyTo(Snapshot<Doujinshi> snapshot, JsonSerializer serializer)
            => ApplyToInternal<Doujinshi, DbDoujinshi>(snapshot, serializer, d => d.ApplyTo(new Doujinshi()));

        public Snapshot<BooruPost> ApplyTo(Snapshot<BooruPost> snapshot, JsonSerializer serializer)
            => ApplyToInternal<BooruPost, DbBooruPost>(snapshot, serializer, p => p.ApplyTo(new BooruPost()));

        Snapshot<T> ApplyToInternal<T, TProjection>(Snapshot<T> snapshot,
                                                    JsonSerializer serializer,
                                                    Func<TProjection, T> project)
        {
            snapshot.Id          = Id.ToGuid();
            snapshot.TargetId    = TargetId.ToGuid();
            snapshot.CommitterId = CommitterId.ToGuid();
            snapshot.Time        = Time;
            snapshot.Event       = Event;
            snapshot.Reason      = Reason ?? snapshot.Reason;

            snapshot.Value = Value == null ? snapshot.Value : project(serializer.Deserialize<TProjection>(Value));

            return snapshot;
        }
    }
}