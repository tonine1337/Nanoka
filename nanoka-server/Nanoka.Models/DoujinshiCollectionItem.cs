using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class DoujinshiCollectionItem
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("doujinshi")]
        public Doujinshi Doujinshi { get; set; }
    }
}