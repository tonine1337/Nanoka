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
        public int PageCount { get; set; }
    }

    public class DoujinshiVariantBase
    {
        /// <summary>
        /// CID referencing the directory containing image files of this variant.
        /// </summary>
        [JsonProperty("cid"), Required]
        public string Cid { get; set; }

        [JsonProperty("metas"), Required]
        public Dictionary<DoujinshiMeta, string[]> Metas { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }
    }
}