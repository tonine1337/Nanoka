using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    /// <summary>
    /// Represents a generic book.
    /// </summary>
    public class Book : BookBase, IHasId, IHasScore
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("contents")]
        public BookContent[] Contents { get; set; }
    }

    public class BookBase : IHasEntityType, IValidatableObject
    {
        public const int MinimumTagCount = 5;

        /// <summary>
        /// First element should be the fully localized primary name.
        /// </summary>
        [JsonProperty("names"), Required, MinLength(1)]
        public string[] Name { get; set; }

        [JsonProperty("tags"), Required]
        public Dictionary<BookTag, string[]> Tags { get; set; }

        [JsonProperty("category")]
        public BookCategory Category { get; set; }

        [JsonProperty("rating")]
        public MaterialRating Rating { get; set; }

#region Meta

        [JsonIgnore]
        NanokaEntity IHasEntityType.Type => NanokaEntity.Book;

#endregion

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Tags.Values.SelectMany(x => x).Count() < MinimumTagCount)
                yield return new ValidationResult($"Must provide at least {MinimumTagCount} tags.");
        }
    }
}