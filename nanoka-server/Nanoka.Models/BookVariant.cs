using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class BookVariant : BookVariantBase
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("uploader")]
        public Guid UploaderId { get; set; }

        [JsonProperty("pages")]
        public int PageCount { get; set; }
    }

    public class BookVariantBase
    {
        [JsonProperty("name"), Required]
        public string Name { get; set; }

        [JsonProperty("name_romanized")]
        public string RomanizedName { get; set; }

        [JsonProperty("language"), EnumDataType(typeof(LanguageType))]
        public LanguageType Language { get; set; }

        [JsonProperty("source"), Uri]
        public string Source { get; set; }
    }
}