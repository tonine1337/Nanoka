using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class DoujinshiCollection
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("items")]
        public DoujinshiCollectionItem[] Items { get; set; }
    }
}
