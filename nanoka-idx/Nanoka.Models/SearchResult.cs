using Newtonsoft.Json;

namespace Nanoka.Models
{
    public sealed class SearchResult<T>
    {
        [JsonProperty("took")]
        public long Took { get; set; }

        [JsonProperty("tookAccurate")]
        public double TookAccurate { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("items")]
        public T[] Items { get; set; }
    }
}