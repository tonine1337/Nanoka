using Newtonsoft.Json;

namespace Nanoka.Models.Query
{
    public struct FilterQuery<T> : ISearchQuery
    {
        [JsonProperty("strictness")]
        public QueryStrictness Strictness { get; set; }

        [JsonProperty("operator")]
        public QueryOperator Operator { get; set; }

        [JsonProperty("values")]
        public T[] Values { get; set; }

        public bool IsSpecified() => Values != null && Values.Length != 0;

        public static implicit operator FilterQuery<T>(T value) => new FilterQuery<T>
        {
            Strictness = QueryStrictness.Must,
            Operator   = QueryOperator.All,
            Values     = new[] { value }
        };

        public static implicit operator FilterQuery<T>(T[] values) => new FilterQuery<T>
        {
            Strictness = QueryStrictness.Must,
            Operator   = QueryOperator.Any,
            Values     = values
        };
    }
}