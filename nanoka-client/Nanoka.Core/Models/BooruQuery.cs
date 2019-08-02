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
        /// Queries against all text fields.
        /// </summary>
        [JsonProperty("multi")]
        public TextQuery Multi { get; set; }

        [JsonProperty("upload")]
        public RangeQuery<DateTime> UploadTime { get; set; }

        [JsonProperty("update")]
        public RangeQuery<DateTime> UpdateTime { get; set; }

        [JsonProperty("tags")]
        public IDictionary<BooruTagType, TextQuery> Tags { get; set; }

        [JsonProperty("rating")]
        public FilterQuery<BooruRating> Rating { get; set; }

        [JsonProperty("score")]
        public RangeQuery<int> Score { get; set; }

        [JsonProperty("src")]
        public TextQuery Source { get; set; }

        [JsonProperty("w")]
        public RangeQuery<int> Width { get; set; }

        [JsonProperty("h")]
        public RangeQuery<int> Height { get; set; }

        [JsonProperty("s")]
        public RangeQuery<int> SizeInBytes { get; set; }

        [JsonProperty("t")]
        public FilterQuery<string> MediaType { get; set; }

        [JsonProperty("sorting")]
        public BooruQuerySort Sorting { get; set; }
    }
}