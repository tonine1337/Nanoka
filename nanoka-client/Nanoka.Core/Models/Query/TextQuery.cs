using Newtonsoft.Json;

namespace Nanoka.Core.Models.Query
{
    public struct TextQuery
    {
        [JsonProperty("strictness")]
        public QueryStrictness Strictness { get; set; }

        [JsonProperty("operator")]
        public QueryOperator Operator { get; set; }

        [JsonProperty("mode")]
        public TextQueryMode Mode { get; set; }

        [JsonProperty("values")]
        public string[] Values { get; set; }
    }
}