using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class BooruTag
    {
        [JsonProperty("t")]
        public BooruTagType Type { get; set; }

        [JsonProperty("v")]
        public string Value { get; set; }
    }
}