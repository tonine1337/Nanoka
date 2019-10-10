using System;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class UploadState
    {
        /// <summary>
        /// Upload ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Time when this task started.
        /// </summary>
        [JsonProperty("start")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// ID of the user that created this task.
        /// </summary>
        [JsonProperty("uploader")]
        public string UploaderId { get; set; }

        /// <summary>
        /// Number of files added to this task.
        /// </summary>
        [JsonProperty("fileCount")]
        public int FileCount { get; set; }
    }
}