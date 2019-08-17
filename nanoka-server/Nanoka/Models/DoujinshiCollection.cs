using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class DoujinshiCollection
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("items")]
        public DoujinshiCollectionItem[] Items { get; set; }
    }
}