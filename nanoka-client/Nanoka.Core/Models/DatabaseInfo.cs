using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class DatabaseInfo
    {
        [JsonProperty("version")]
        public NanokaVersion Version { get; set; }
    }
}
