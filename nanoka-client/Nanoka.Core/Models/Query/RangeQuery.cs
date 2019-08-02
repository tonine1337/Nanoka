using Newtonsoft.Json;

namespace Nanoka.Core.Models.Query
{
    public struct RangeQuery<T> where T : struct
    {
        [JsonProperty("strictness")]
        public QueryStrictness Strictness { get; set; }

        [JsonProperty("operator")]
        public QueryOperator Operator { get; set; }

        [JsonProperty("values")]
        public RangeQueryItem<T>[] Values { get; set; }
    }
}