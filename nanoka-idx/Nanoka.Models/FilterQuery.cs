using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public struct FilterQuery<T> : ISearchQuery
    {
        [JsonProperty("value")]
        public T[] Values { get; set; }

        [JsonProperty("mode")]
        public QueryMatchMode Mode { get; set; }

        [JsonIgnore]
        public bool IsSpecified => Values != null && Values.Length != 0;

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
