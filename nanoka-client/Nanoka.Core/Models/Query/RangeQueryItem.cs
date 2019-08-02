using Newtonsoft.Json;

namespace Nanoka.Core.Models.Query
{
    public class RangeQueryItem<T> where T : struct
    {
        [JsonProperty("min")]
        public T? Min { get; set; }

        [JsonProperty("max")]
        public T? Max { get; set; }

        [JsonProperty("exclusive")]
        public bool Exclusive { get; set; }
    }
}