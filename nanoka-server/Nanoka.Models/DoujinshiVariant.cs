using System;
using System.Collections.Generic;
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
        [JsonProperty("metas"), Required]
        public Dictionary<DoujinshiMeta, string[]> Metas { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }
    }
}