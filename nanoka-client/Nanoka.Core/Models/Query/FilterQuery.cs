using Newtonsoft.Json;

namespace Nanoka.Core.Models.Query
{
    public struct FilterQuery<T>
    {
        [JsonProperty("strictness")]
        public QueryStrictness Strictness { get; set; }

        [JsonProperty("operator")]
        public QueryOperator Operator { get; set; }

        [JsonProperty("values")]
        public T[] Values { get; set; }
    }
}