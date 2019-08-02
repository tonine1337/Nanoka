using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class DoujinshiMeta
    {
        [JsonProperty("t")]
        public DoujinshiMetaType Type { get; set; }

        [JsonProperty("v")]
        public string Value { get; set; }
    }
}
