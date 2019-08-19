using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class DoujinshiVariant : DoujinshiVariantBase
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("uploader")]
        public Guid UploaderId { get; set; }

        [JsonProperty("pages")]
        public int PageCount { get; set; }
    }

    public class DoujinshiVariantBase
    {
        [JsonProperty("name"), Required]
        public string Name { get; set; }

        [JsonProperty("name_romanized")]
        public string RomanizedName { get; set; }

        [JsonProperty("language")]
        public LanguageType Language { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }
    }
}