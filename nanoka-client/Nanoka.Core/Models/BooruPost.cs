using System;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class BooruPost
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("upload")]
        public DateTime UploadTime { get; set; }

        [JsonProperty("update")]
        public DateTime UpdateTime { get; set; }

        [JsonProperty("tags")]
        public BooruTag[] Tags { get; set; }

        [JsonProperty("rate")]
        public BooruRating Rating { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("img")]
        public BooruImage Image { get; set; }

        [JsonProperty("sib")]
        public Guid[] SiblingIds { get; set; }
    }
}