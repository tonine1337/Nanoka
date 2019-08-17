using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Nanoka.Models.Query
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

        public static implicit operator RangeQuery<T>((T min, T max) range) => new RangeQuery<T>
        {
            Strictness = QueryStrictness.Must,
            Operator   = QueryOperator.All,

            Values = new[]
            {
                new RangeQueryItem<T>
                {
                    Min = range.min,
                    Max = range.max
                }
            }
        };

        public static implicit operator RangeQuery<T>((T min, T max)[] ranges) => new RangeQuery<T>
        {
            Strictness = QueryStrictness.Must,
            Operator   = QueryOperator.Any,

            Values = ranges.ToArray(x => new RangeQueryItem<T>
            {
                Min = x.min,
                Max = x.max
            })
        };

        public static implicit operator RangeQuery<T>(KeyValuePair<T, T>[] pairs) => new RangeQuery<T>
        {
            Strictness = QueryStrictness.Must,
            Operator   = QueryOperator.Any,

            Values = pairs.ToArray(x => new RangeQueryItem<T>
            {
                Min = x.Key,
                Max = x.Value
            })
        };
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