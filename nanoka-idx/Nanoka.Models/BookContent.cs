using Newtonsoft.Json;

namespace Nanoka.Models
{
    /// <summary>
    /// Represents the contents of a book.
    /// </summary>
    public class BookContent : BookContentBase, IHasId
    {
        /// <summary>
        /// Content ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Number of pages in this content.
        /// </summary>
        [JsonProperty("pageCount")]
        public int PageCount { get; set; }
    }

    public class BookContentBase
    {
        /// <summary>
        /// Content language.
        /// </summary>
        [JsonProperty("language")]
        public LanguageType Language { get; set; }

        /// <summary>
        /// Whether the content is colored or black-and-white.
        /// </summary>
        [JsonProperty("color")]
        public bool IsColor { get; set; }

        /// <summary>
        /// Sources from where this content was downloaded from.
        /// </summary>
        [JsonProperty("source")]
        public ExternalSource[] Sources { get; set; }
    }
}