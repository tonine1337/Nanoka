using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models.Requests
{
    public class CreateNewBookRequest : IValidatableObject
    {
        /// <summary>
        /// New book information to add.
        /// If this is specified, <see cref="BookId"/> must be null.
        /// </summary>
        [JsonProperty("book")]
        public BookBase Book { get; set; }

        /// <summary>
        /// Existing book identifier to merge <see cref="Content"/> into.
        /// If this is specified, <see cref="Book"/> must be null.
        /// </summary>
        [JsonProperty("book_id")]
        public string BookId { get; set; }

        /// <summary>
        /// Content of the book to add.
        /// </summary>
        [JsonProperty("content"), Required]
        public BookContentBase Content { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Book == null && BookId == null)
                yield return new ValidationResult("Either 'book' or 'book_id' must be specified.");

            if (Book != null && BookId != null)
                yield return new ValidationResult("May not specify both 'book' and 'book_id'.");
        }
    }
}