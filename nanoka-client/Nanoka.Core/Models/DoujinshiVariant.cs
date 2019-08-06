using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class DoujinshiVariant : DoujinshiVariantBase
    {
        [JsonProperty("uploader")]
        public Guid UploaderId { get; set; }

        [JsonProperty("pages")]
        public DoujinshiPage[] Pages { get; set; }
    }

    public class DoujinshiVariantBase
    {
        [JsonProperty("metas"), Required]
        public IDictionary<DoujinshiMeta, string[]> Metas { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }
    }
}