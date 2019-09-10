using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models
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

        [JsonProperty("type")]
        public string MediaType { get; set; }
    }

    public class BooruPostBase
    {
        [JsonProperty("tags"), Required]
        public Dictionary<BooruTag, string[]> Tags { get; set; }

        [JsonProperty("rating"), EnumDataType(typeof(BooruRating))]
        public BooruRating Rating { get; set; }

        [JsonProperty("source"), Uri]
        public string Source { get; set; }

        [JsonProperty("siblings")]
        public List<Guid> SiblingIds { get; set; }
    }
}