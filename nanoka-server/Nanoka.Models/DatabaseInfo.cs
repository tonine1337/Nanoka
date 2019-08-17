using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class DatabaseInfo
    {
        [JsonProperty("version")]
        public NanokaVersion Version { get; set; }
    }
}