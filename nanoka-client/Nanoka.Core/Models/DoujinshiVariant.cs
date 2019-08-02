using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class DoujinshiVariant
    {
        [JsonProperty("cid")]
        public string Cid { get; set; }

        [JsonProperty("metas")]
        public DoujinshiMeta[] Metas { get; set; }

        [JsonProperty("src")]
        public string Source { get; set; }

        [JsonProperty("pages")]
        public DoujinshiPage[] Pages { get; set; }
    }
}
