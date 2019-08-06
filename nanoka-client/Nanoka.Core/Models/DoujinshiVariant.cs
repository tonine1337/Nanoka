using System;
using System.Collections.Generic;
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
        [JsonProperty("metas")]
        public IDictionary<DoujinshiMeta, string[]> Metas { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }
    }
}