using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class BooruEntry
    {
        /// <summary>
        /// The CID that refers to this doujinshi object.
        /// </summary>
        [JsonIgnore]
        public string Cid { get; set; }

        [JsonProperty("metas")]
        public BooruMeta[] Metas { get; set; }

        [JsonProperty("rate")]
        public BooruRating Rating { get; set; }

        [JsonProperty("sc")]
        public int Score { get; set; }

        [JsonProperty("img")]
        public BooruImage Image { get; set; }
    }
}