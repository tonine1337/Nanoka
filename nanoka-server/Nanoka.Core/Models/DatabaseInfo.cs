using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class DatabaseInfo
    {
        [JsonProperty("version")]
        public NanokaVersion Version { get; set; }

        [JsonProperty("ipfs_bootstrap")]
        public string IpfsBootstrap { get; set; }

        [JsonProperty("ipfs_swarm")]
        public string IpfsSwarmKey { get; set; }
    }
}
