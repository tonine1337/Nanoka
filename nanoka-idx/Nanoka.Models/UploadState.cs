using System;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class UploadState
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("start")]
        public DateTime StartTime { get; set; }

        [JsonProperty("uploader")]
        public string UploaderId { get; set; }

        [JsonProperty("file_count")]
        public int FileCount { get; set; }
    }
}