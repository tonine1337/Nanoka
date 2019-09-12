using System;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class Vote : VoteBase
    {
        [JsonProperty("time")]
        public DateTime Time { get; set; }
    }

    public class VoteBase : IHasEntityType
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("entity_type")]
        public NanokaEntity EntityType { get; set; }

        [JsonProperty("entity_id")]
        public int EntityId { get; set; }

        [JsonProperty("type")]
        public VoteType Type { get; set; }

#region Meta

        [JsonIgnore]
        NanokaEntity IHasEntityType.Type => NanokaEntity.Vote;

#endregion
    }
}