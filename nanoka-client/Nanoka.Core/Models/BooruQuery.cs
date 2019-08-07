using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nanoka.Core.Models.Query;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class BooruQuery
    {
        [JsonProperty("offset"), Range(0, int.MaxValue)]
        public int Offset { get; set; }

        [JsonProperty("limit"), Range(1, int.MaxValue), Required]
        public int Limit { get; set; }

        /// <summary>
        /// Queries against all fields.
        /// </summary>
        [JsonProperty("all")]
        public TextQuery All { get; set; }

        [JsonProperty("upload")]
        public RangeQuery<DateTime> UploadTime { get; set; }

        [JsonProperty("update")]
        public RangeQuery<DateTime> UpdateTime { get; set; }

        [JsonProperty("tags")]
        public Dictionary<BooruTag, TextQuery> Tags { get; set; }

        [JsonProperty("rating")]
        public FilterQuery<BooruRating> Rating { get; set; }

        [JsonProperty("score")]
        public RangeQuery<int> Score { get; set; }

        [JsonProperty("source")]
        public TextQuery Source { get; set; }

        [JsonProperty("width")]
        public RangeQuery<int> Width { get; set; }

        [JsonProperty("height")]
        public RangeQuery<int> Height { get; set; }

        [JsonProperty("type")]
        public TextQuery MediaType { get; set; }

        [JsonProperty("sorting"), Required]
        public List<BooruQuerySort> Sorting { get; set; }
    }
}