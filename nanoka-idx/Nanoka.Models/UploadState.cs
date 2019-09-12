using System;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class UploadState
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("start")]
        public DateTime StartTime { get; set; }

        [JsonProperty("uploader")]
        public int UploaderId { get; set; }

        [JsonProperty("file_count")]
        public int FileCount { get; set; }
    }
}