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
        /// <summary>
        /// Book ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Score value.
        /// </summary>
        [JsonProperty("score")]
        public double Score { get; set; }

        /// <summary>
        /// Contents in this book.
        /// </summary>
        [JsonProperty("contents")]
        public BookContent[] Contents { get; set; }
    }

    public class BookBase : IHasEntityType, IValidatableObject
    {
        public const int MinimumTagCount = 5;

        /// <summary>
        /// Book names.
        /// </summary>
        /// <remarks>
        /// First element should be fully localized primary name.
        /// </remarks>
        [JsonProperty("names"), Required, MinLength(1)]
        public string[] Name { get; set; }

        /// <summary>
        /// Tags added to this book.
        /// </summary>
        [JsonProperty("tags"), Required]
        public Dictionary<BookTag, string[]> Tags { get; set; }

        /// <summary>
        /// Book category.
        /// </summary>
        [JsonProperty("category")]
        public BookCategory Category { get; set; }

        /// <summary>
        /// Content rating.
        /// </summary>
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