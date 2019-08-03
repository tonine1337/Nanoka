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
}
