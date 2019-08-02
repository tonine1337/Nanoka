using System;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class Doujinshi
    {
        [JsonProperty("upload")]
        public DateTime UploadTime { get; set; }

        [JsonProperty("update")]
        public DateTime UpdateTime { get; set; }

        [JsonProperty("name")]
        public string OriginalName { get; set; }

        [JsonProperty("roma_name")]
        public string RomanizedName { get; set; }

        [JsonProperty("eng_name")]
        public string EnglishName { get; set; }

        [JsonProperty("src")]
        public string Source { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("metas")]
        public DoujinshiMeta[] Metas { get; set; }

        [JsonProperty("pages")]
        public DoujinshiPage[] Pages { get; set; }
    }
}
