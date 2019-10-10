using System;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class Vote : VoteBase
    {
        /// <summary>
        /// Time when this vote was created.
        /// </summary>
        [JsonProperty("time")]
        public DateTime Time { get; set; }

        /// <summary>
        /// ID of the user that voted.
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; }

        /// <summary>
        ///Type of the entity that this vote targets.
        /// </summary>
        [JsonProperty("entityType")]
        public NanokaEntity EntityType { get; set; }

        /// <summary>
        /// ID of the entity that this vote targets.
        /// </summary>
        [JsonProperty("entityId")]
        public string EntityId { get; set; }

        /// <summary>
        /// Vote weight.
        /// </summary>
        /// <remarks>
        /// Votes can have varying weights depending on the reputation of the voter.
        /// </remarks>
        [JsonProperty("weight")]
        public double Weight { get; set; }

        public override string ToString() => $"{EntityType} {EntityId} ~ {Weight:+##.##;-##.##}";
    }

    public class VoteBase : IHasEntityType
    {
        [JsonProperty("type")]
        public VoteType Type { get; set; }

#region Meta

        [JsonIgnore]
        NanokaEntity IHasEntityType.Type => NanokaEntity.Vote;

#endregion
    }
}