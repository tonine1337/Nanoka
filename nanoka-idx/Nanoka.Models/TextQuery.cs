using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public struct TextQuery : ISearchQuery
    {
        /// <summary>
        /// Values to match.
        /// </summary>
        [JsonProperty("value")]
        public string[] Values { get; set; }

        /// <summary>
        /// Match mode.
        /// </summary>
        [JsonProperty("mode")]
        public QueryMatchMode Mode { get; set; }

        [JsonIgnore]
        public bool IsSpecified => Values != null && Values.Any(v => !string.IsNullOrEmpty(v));

        public static implicit operator TextQuery(string value)
            => new TextQuery { Values = string.IsNullOrEmpty(value) ? null : new[] { value } };

        public static implicit operator TextQuery(string[] values)
            => new TextQuery { Values = values };

        public static implicit operator TextQuery(List<string> values)
            => new TextQuery { Values = values?.ToArray() };

        public static implicit operator TextQuery((string a, string b) x)
            => new TextQuery { Values = new[] { x.a, x.b } };

        public static implicit operator TextQuery((string a, string b, string c) x)
            => new TextQuery { Values = new[] { x.a, x.b, x.c } };

        public static implicit operator TextQuery((string a, string b, string c, string d) x)
            => new TextQuery { Values = new[] { x.a, x.b, x.c, x.d } };

        public static implicit operator TextQuery((string a, string b, string c, string d, string e) x)
            => new TextQuery { Values = new[] { x.a, x.b, x.c, x.d, x.e } };

        public static implicit operator TextQuery((string a, string b, string c, string d, string e, string f) x)
            => new TextQuery { Values = new[] { x.a, x.b, x.c, x.d, x.e, x.f } };

        public static implicit operator TextQuery((string a, string b, string c, string d, string e, string f, string g) x)
            => new TextQuery { Values = new[] { x.a, x.b, x.c, x.d, x.e, x.f, x.g } };
    }
}