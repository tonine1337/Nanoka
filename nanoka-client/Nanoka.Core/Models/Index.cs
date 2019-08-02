using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class Index
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("desc")]
        public string Description { get; set; }

        [JsonProperty("ep")]
        public string Endpoint { get; set; }

        [JsonProperty("type")]
        public IndexType Type { get; set; }
    }
}
