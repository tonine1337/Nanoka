using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class Database
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }
    }
}