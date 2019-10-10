using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public struct FilterQuery<T> : ISearchQuery
    {
        /// <summary>
        /// Values to match.
        /// </summary>
        [JsonProperty("value")]
        public T[] Values { get; set; }

        /// <summary>
        /// Match mode.
        /// </summary>
        [JsonProperty("mode")]
        public QueryMatchMode Mode { get; set; }

        [JsonIgnore]
        public bool IsSpecified => Values != null && Values.Length != 0;

        public FilterQuery<T2> Project<T2>(Func<T, T2> projection) => new FilterQuery<T2>
        {
            Values = Values?.Select(projection).ToArray(),
            Mode   = Mode
        };

        public static implicit operator FilterQuery<T>(T value)
            => new FilterQuery<T> { Values = new[] { value } };

        public static implicit operator FilterQuery<T>(T[] values)
            => new FilterQuery<T> { Values = values };

        public static implicit operator FilterQuery<T>(List<T> values)
            => new FilterQuery<T> { Values = values?.ToArray() };

        public static implicit operator FilterQuery<T>((T a, T b) x)
            => new FilterQuery<T> { Values = new[] { x.a, x.b } };

        public static implicit operator FilterQuery<T>((T a, T b, T c) x)
            => new FilterQuery<T> { Values = new[] { x.a, x.b, x.c } };

        public static implicit operator FilterQuery<T>((T a, T b, T c, T d) x)
            => new FilterQuery<T> { Values = new[] { x.a, x.b, x.c, x.d } };

        public static implicit operator FilterQuery<T>((T a, T b, T c, T d, T e) x)
            => new FilterQuery<T> { Values = new[] { x.a, x.b, x.c, x.d, x.e } };

        public static implicit operator FilterQuery<T>((T a, T b, T c, T d, T e, T f) x)
            => new FilterQuery<T> { Values = new[] { x.a, x.b, x.c, x.d, x.e, x.f } };

        public static implicit operator FilterQuery<T>((T a, T b, T c, T d, T e, T f, T g) x)
            => new FilterQuery<T> { Values = new[] { x.a, x.b, x.c, x.d, x.e, x.f, x.g } };
    }
}