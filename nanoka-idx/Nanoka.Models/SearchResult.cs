using Newtonsoft.Json;

namespace Nanoka.Models
{
    public sealed class SearchResult<T>
    {
        /// <summary>
        /// How long it took for the database to complete the search.
        /// </summary>
        [JsonProperty("took")]
        public long Took { get; set; }

        /// <summary>
        /// How long it took for the request to be completely processed.
        /// </summary>
        [JsonProperty("tookAccurate")]
        public double TookAccurate { get; set; }

        /// <summary>
        /// Total number of matched items.
        /// </summary>
        [JsonProperty("total")]
        public long Total { get; set; }

        /// <summary>
        /// Search results.
        /// </summary>
        [JsonProperty("items")]
        public T[] Items { get; set; }
    }
}