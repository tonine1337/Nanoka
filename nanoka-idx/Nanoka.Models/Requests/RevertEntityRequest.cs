using Newtonsoft.Json;

namespace Nanoka.Models.Requests
{
    public class RevertEntityRequest
    {
        [JsonProperty("snapshotId")]
        public string SnapshotId { get; set; }
    }
}