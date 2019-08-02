using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class DoujinshiGroup
    {
        [JsonProperty("items")]
        public Doujinshi[] Items { get; set; }
    }
}