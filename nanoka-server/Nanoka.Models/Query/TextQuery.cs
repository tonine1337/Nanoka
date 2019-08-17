using Newtonsoft.Json;

namespace Nanoka.Models.Query
{
    public struct TextQuery : ISearchQuery
    {
        [JsonProperty("strictness")]
        public QueryStrictness Strictness { get; set; }

        [JsonProperty("operator")]
        public QueryOperator Operator { get; set; }

        [JsonProperty("values")]
        public string[] Values { get; set; }

        public bool IsSpecified() => Values != null && Values.Length != 0;

        public static implicit operator TextQuery(string value) => new TextQuery
        {
            Strictness = QueryStrictness.Must,
            Operator   = QueryOperator.All,
            Values     = new[] { value }
        };

        public static implicit operator TextQuery(string[] values) => new TextQuery
        {
            Strictness = QueryStrictness.Must,
            Operator   = QueryOperator.Any,
            Values     = values
        };
    }
}