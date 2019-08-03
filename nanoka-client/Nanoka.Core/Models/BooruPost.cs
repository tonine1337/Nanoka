using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class BooruPost
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("uploader")]
        public Guid UploaderId { get; set; }

        [JsonProperty("cid")]
        public string Cid { get; set; }

        [JsonProperty("upload")]
        public DateTime UploadTime { get; set; }

        [JsonProperty("update")]
        public DateTime UpdateTime { get; set; }

        [JsonProperty("tags")]
        public IDictionary<BooruTag, string[]> Tags { get; set; }

        [JsonProperty("rating")]
        public BooruRating Rating { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("size")]
        public int SizeInBytes { get; set; }

        [JsonProperty("type")]
        public string MediaType { get; set; }

        [JsonProperty("siblings")]
        public Guid[] SiblingIds { get; set; }
    }
}