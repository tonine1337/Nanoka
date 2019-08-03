using Newtonsoft.Json;

namespace Nanoka.Core.Models.Query
{
    public struct TextQuery : ISearchQuery
    {
        [JsonProperty("strictness")]
        public QueryStrictness Strictness { get; set; }

        [JsonProperty("operator")]
        public QueryOperator Operator { get; set; }

        [JsonProperty("mode")]
        public TextQueryMode Mode { get; set; }

        [JsonProperty("values")]
        public string[] Values { get; set; }

        public bool IsSpecified() => Values != null && Values.Length != 0;
    }
}
