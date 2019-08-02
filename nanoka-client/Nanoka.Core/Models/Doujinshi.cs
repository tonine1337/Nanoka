using System;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class Doujinshi
    {
        /// <summary>
        /// The CID that refers to this doujinshi object.
        /// </summary>
        [JsonIgnore]
        public string Cid { get; set; }

        [JsonProperty("ut")]
        public DateTime UploadTime { get; set; }

        [JsonProperty("pt")]
        public DateTime ProcessTime { get; set; }

        [JsonProperty("on")]
        public string OriginalName { get; set; }

        [JsonProperty("rn")]
        public string RomanizedName { get; set; }

        [JsonProperty("en")]
        public string EnglishName { get; set; }

        [JsonProperty("src")]
        public string Source { get; set; }

        [JsonProperty("metas")]
        public DoujinshiMeta[] Metas { get; set; }
    }
}
