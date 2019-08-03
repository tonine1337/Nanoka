using System;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class Doujinshi
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("upload")]
        public DateTime UploadTime { get; set; }

        [JsonProperty("update")]
        public DateTime UpdateTime { get; set; }

        [JsonProperty("name")]
        public string OriginalName { get; set; }

        [JsonProperty("name_roma")]
        public string RomanizedName { get; set; }

        [JsonProperty("name_eng")]
        public string EnglishName { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("variations")]
        public DoujinshiVariant[] Variations { get; set; }
    }
}