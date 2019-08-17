using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class SearchResult<T>
    {
        /// <summary>
        /// Total number of matched items.
        /// </summary>
        [JsonProperty("total")]
        public int Total { get; set; }

        /// <summary>
        /// Length of time it took for the search to complete, in milliseconds.
        /// </summary>
        [JsonProperty("took")]
        public double Took { get; set; }

        [JsonProperty("result")]
        public T[] Items { get; set; }
    }
}