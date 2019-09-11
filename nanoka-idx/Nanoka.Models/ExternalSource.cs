using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class ExternalSource : IEquatable<ExternalSource>
    {
        [JsonProperty("website"), Required]
        public string Website { get; set; }

        [JsonProperty("identifier"), Required]
        public string Identifier { get; set; }

        public override bool Equals(object obj) => obj is ExternalSource src && Equals(src);

        public bool Equals(ExternalSource other) => other != null &&
                                                    Website == other.Website &&
                                                    Identifier == other.Identifier;

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                return ((Website != null ? Website.GetHashCode() : 0) * 397) ^ (Identifier != null ? Identifier.GetHashCode() : 0);
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        /// <summary>
        /// Parses the format "(website)/(identifier)".
        /// </summary>
        public static ExternalSource Parse(string combined)
        {
            var parts = combined.Split(new[] { '/' }, 2);

            return new ExternalSource
            {
                Website    = parts[0],
                Identifier = parts.Length != 1 ? parts[1] : null
            };
        }

        public override string ToString() => $"{Website}/{Identifier}";
    }
}