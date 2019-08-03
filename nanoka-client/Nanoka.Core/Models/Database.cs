using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class Database
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("desc")]
        public string Description { get; set; }

        [JsonProperty("ep")]
        public string Endpoint { get; set; }
    }
}
