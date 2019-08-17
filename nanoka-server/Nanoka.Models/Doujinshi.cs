using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class Doujinshi : DoujinshiBase
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("upload")]
        public DateTime UploadTime { get; set; }

        [JsonProperty("update")]
        public DateTime UpdateTime { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("variants")]
        public List<DoujinshiVariant> Variants { get; set; }
    }

    public class DoujinshiBase
    {
        [JsonProperty("name_original"), Required]
        public string OriginalName { get; set; }

        [JsonProperty("name_romanized")]
        public string RomanizedName { get; set; }

        [JsonProperty("name_english")]
        public string EnglishName { get; set; }

        [JsonProperty("category")]
        public DoujinshiCategory Category { get; set; }
    }
}