using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nanoka.Core.Models.Query;
using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class DoujinshiQuery
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

        [JsonProperty("sorting"), Required]
        public DoujinshiQuerySort[] Sorting { get; set; }
    }
}