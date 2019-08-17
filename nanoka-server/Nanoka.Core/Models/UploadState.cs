using System;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class UploadState
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("progress")]
        public double Progress { get; set; }

        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("end")]
        public DateTime End { get; set; }

        [JsonProperty("running")]
        public bool IsRunning { get; set; }

        [JsonProperty("failed")]
        public bool IsFailed { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}