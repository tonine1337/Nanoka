using System;
using Nanoka.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Database
{
    [ElasticsearchType(IdProperty = nameof(Id), RelationName = nameof(Vote))]
    public class DbVote
    {
        // this is similar to a composite key
        // the format is: userId_entityType_entityId
        [Keyword(Name = "id", Index = false), JsonProperty("id")]
        public string Id { get; set; }

        [Keyword(Name = "u"), JsonProperty("u")]
        public int UserId { get; set; }

        [Keyword(Name = "x"), JsonProperty("e")]
        public NanokaEntity EntityType { get; set; }

        [Keyword(Name = "e"), JsonProperty("e")]
        public int EntityId { get; set; }

        [Keyword(Name = "y"), JsonProperty("x")]
        public VoteType Type { get; set; }

        [Date(Name = "t"), JsonProperty("t")]
        public DateTime Time { get; set; }

        public Vote ToVote() => new Vote
        {
            UserId     = UserId,
            EntityType = EntityType,
            EntityId   = EntityId,
            Type       = Type,
            Time       = Time
        };

        public static DbVote FromVote(Vote vote) => new DbVote
        {
            Id         = CreateId(vote.UserId, vote.EntityType, vote.EntityId),
            UserId     = vote.UserId,
            EntityType = vote.EntityType,
            EntityId   = vote.EntityId,
            Type       = vote.Type,
            Time       = vote.Time
        };

        public static string CreateId(int userId, NanokaEntity entity, int entityId)
            => $"{userId}_{ExtensionsEnum<NanokaEntity>.GetName(entity)}_{entityId}";
    }
}