using System;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class UploadState
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("update")]
        public DateTime Update { get; set; }

        [JsonProperty("count")]
        public int ItemCount { get; set; }

        [JsonProperty("active")]
        public bool IsActive { get; set; }
    }
}