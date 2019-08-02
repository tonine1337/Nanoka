using System;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class BooruEntry
    {
        [JsonProperty("upload")]
        public DateTime UploadTime { get; set; }

        [JsonProperty("update")]
        public DateTime UpdateTime { get; set; }

        [JsonProperty("metas")]
        public BooruMeta[] Metas { get; set; }

        [JsonProperty("rate")]
        public BooruRating Rating { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("img")]
        public BooruImage Image { get; set; }
    }
}
