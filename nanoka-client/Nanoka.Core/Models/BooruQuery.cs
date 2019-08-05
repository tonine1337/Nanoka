using System;
using System.Collections.Generic;
using Nanoka.Core.Models.Query;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class BooruQuery
    {
        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("limit")]
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
        public IDictionary<BooruTag, TextQuery> Tags { get; set; }

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

        [JsonProperty("size")]
        public RangeQuery<int> SizeInBytes { get; set; }

        [JsonProperty("type")]
        public FilterQuery<string> MediaType { get; set; }

        [JsonProperty("sorting")]
        public BooruQuerySort Sorting { get; set; }
    }
}