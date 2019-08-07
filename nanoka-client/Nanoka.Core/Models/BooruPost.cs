using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class BooruPost : BooruPostBase
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("uploader")]
        public Guid UploaderId { get; set; }

        [JsonProperty("upload")]
        public DateTime UploadTime { get; set; }

        [JsonProperty("update")]
        public DateTime UpdateTime { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("size")]
        public int SizeInBytes { get; set; }

        [JsonProperty("type")]
        public string MediaType { get; set; }
    }

    public class BooruPostBase
    {
        /// <summary>
        /// CID referencing the image file of this post.
        /// </summary>
        [JsonProperty("cid"), Required]
        public string Cid { get; set; }

        [JsonProperty("tags"), Required]
        public Dictionary<BooruTag, string[]> Tags { get; set; }

        [JsonProperty("rating")]
        public BooruRating Rating { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("siblings")]
        public List<Guid> SiblingIds { get; set; }
    }
}
