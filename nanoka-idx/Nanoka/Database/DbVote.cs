using System;
using Nanoka.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Database
{
    [ElasticsearchType(RelationName = nameof(Vote))]
    public class DbVote : IHasId
    {
        // this is similar to a composite key
        // the format is: userId_entityType_entityId
        [Keyword(Name = "id", Index = false), JsonProperty("id")]
        public string Id { get; set; }

        [Keyword(Name = "u"), JsonProperty("u")]
        public string UserId { get; set; }

        [Keyword(Name = "x"), JsonProperty("e")]
        public NanokaEntity EntityType { get; set; }

        [Keyword(Name = "e"), JsonProperty("e")]
        public string EntityId { get; set; }

        [Keyword(Name = "y", Index = false), JsonProperty("x")]
        public VoteType Type { get; set; }

        [Date(Name = "t", Index = false), JsonProperty("t")]
        public DateTime Time { get; set; }

        [Number(NumberType.Double, Name = "w", Index = false), JsonProperty("w")]
        public double Weight { get; set; }

        public Vote ToVote() => new Vote
        {
            UserId     = UserId,
            EntityType = EntityType,
            EntityId   = EntityId,
            Type       = Type,
            Time       = Time,
            Weight     = Weight
        };

        public static DbVote FromVote(Vote vote) => new DbVote
        {
            Id         = CreateId(vote.UserId, vote.EntityType, vote.EntityId),
            UserId     = vote.UserId,
            EntityType = vote.EntityType,
            EntityId   = vote.EntityId,
            Type       = vote.Type,
            Time       = vote.Time,
            Weight     = vote.Weight
        };

        public static string CreateId(string userId, NanokaEntity entity, string entityId)
            => $"{userId}_{ExtensionsEnum<NanokaEntity>.GetName(entity)}_{entityId}";
    }
}
