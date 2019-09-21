using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public sealed class SearchResult<TEntity>
    {
        [JsonProperty("took")]
        public long Took { get; set; }

        [JsonProperty("took_accurate")]
        public double TookAccurate { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("items")]
        public IReadOnlyList<TEntity> Items { get; set; }
    }
}