using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class DoujinshiVariant
    {
        [JsonProperty("metas")]
        public IDictionary<DoujinshiMeta, string[]> Metas { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("pages")]
        public DoujinshiPage[] Pages { get; set; }
    }
}