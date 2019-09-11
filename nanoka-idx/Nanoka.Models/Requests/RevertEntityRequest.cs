using Newtonsoft.Json;

namespace Nanoka.Models.Requests
{
    public class RevertEntityRequest
    {
        [JsonProperty("snapshot_id")]
        public int SnapshotId { get; set; }
    }
}