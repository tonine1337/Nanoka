using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class BooruMeta
    {
        [JsonProperty("t")]
        public BooruMetaType Type { get; set; }

        [JsonProperty("v")]
        public string Value { get; set; }
    }
}