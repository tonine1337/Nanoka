using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class BookCollectionItem
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("book")]
        public Book Book { get; set; }
    }
}