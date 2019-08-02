using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class DoujinshiCollectionItem
    {
        [JsonProperty("id")]
        public int Index { get; set; }

        [JsonProperty("value")]
        public Doujinshi Doujinshi { get; set; }
    }
}