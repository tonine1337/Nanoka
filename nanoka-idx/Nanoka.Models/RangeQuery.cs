using System;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    /// <remarks>
    /// Ranges are inclusive by default.
    /// </remarks>
    public struct RangeQuery<T> : ISearchQuery where T : struct
    {
        [JsonProperty("min")]
        public T? Minimum { get; set; }

        [JsonProperty("max")]
        public T? Maximum { get; set; }

        [JsonProperty("exclusive")]
        public bool Exclusive { get; set; }

        [JsonIgnore]
        public bool IsSpecified => Minimum != null || Maximum != null;

        public RangeQuery<T2> Project<T2>(Func<T, T2> projection) where T2 : struct => new RangeQuery<T2>
        {
            Minimum   = Minimum == null ? null as T2? : projection(Minimum.Value),
            Maximum   = Maximum == null ? null as T2? : projection(Maximum.Value),
            Exclusive = Exclusive
        };

        public static implicit operator RangeQuery<T>(T? value) => new RangeQuery<T>
        {
            Minimum = value,
            Maximum = value
        };

        public static implicit operator RangeQuery<T>((T? min, T? max) x) => new RangeQuery<T>
        {
            Minimum = x.min,
            Maximum = x.max
        };
    }
}