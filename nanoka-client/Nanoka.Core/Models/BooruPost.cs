using System;
using System.Collections.Generic;
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
        public IDictionary<BooruTag, string[]> Tags { get; set; }

        [JsonProperty("rating")]
        public BooruRating Rating { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("image")]
        public BooruImage Image { get; set; }

        [JsonProperty("siblings")]
        public Guid[] SiblingIds { get; set; }
    }
}