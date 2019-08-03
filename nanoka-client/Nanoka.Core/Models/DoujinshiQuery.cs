using System;
using System.Collections.Generic;
using Nanoka.Core.Models.Query;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class DoujinshiQuery
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

        [JsonProperty("name_original")]
        public TextQuery OriginalName { get; set; }

        [JsonProperty("name_romanized")]
        public TextQuery RomanizedName { get; set; }

        [JsonProperty("name_english")]
        public TextQuery EnglishName { get; set; }

        [JsonProperty("score")]
        public RangeQuery<int> Score { get; set; }

        [JsonProperty("metas")]
        public IDictionary<DoujinshiMeta, TextQuery> Metas { get; set; }

        [JsonProperty("source")]
        public TextQuery Source { get; set; }

        [JsonProperty("pages")]
        public RangeQuery<int> PageCount { get; set; }

        [JsonProperty("sorting")]
        public DoujinshiQuerySort[] Sorting { get; set; }
    }
}