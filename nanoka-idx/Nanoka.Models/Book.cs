using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    /// <summary>
    /// Represents a generic book.
    /// </summary>
    public class Book : BookBase
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("contents")]
        public BookContent[] Contents { get; set; }
    }

    public class BookBase
    {
        public const int MinimumTagCount = 5;

        /// <summary>
        /// First element should be the fully localized primary name.
        /// </summary>
        [JsonProperty("names"), Required, MinLength(1)]
        public string[] Name { get; set; }

        [JsonProperty("tags"), Required, MinLength(MinimumTagCount)]
        public Dictionary<BookTag, string[]> Tags { get; set; }

        [JsonProperty("category")]
        public BookCategory Category { get; set; }

        [JsonProperty("rating")]
        public MaterialRating Rating { get; set; }
    }
}