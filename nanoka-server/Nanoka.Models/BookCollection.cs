using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class BookCollection
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("items")]
        public BookCollectionItem[] Items { get; set; }
    }
}