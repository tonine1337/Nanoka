using System.Linq;
using Newtonsoft.Json;

namespace Nanoka.Core.Models.Query
{
    public struct RangeQuery<T> : ISearchQuery where T : struct
    {
        [JsonProperty("strictness")]
        public QueryStrictness Strictness { get; set; }

        [JsonProperty("operator")]
        public QueryOperator Operator { get; set; }

        [JsonProperty("values")]
        public RangeQueryItem<T>[] Values { get; set; }

        public bool IsSpecified()
            => Values != null &&
               Values.Length != 0 &&
               Values.Any(x => x.Min != null || x.Max != null);
    }

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
