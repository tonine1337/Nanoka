using Newtonsoft.Json;

namespace Nanoka.Models
{
    /// <summary>
    /// Represents the contents of a book.
    /// </summary>
    public class BookContent : BookContentBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("pageCount")]
        public int PageCount { get; set; }
    }

    public class BookContentBase
    {
        [JsonProperty("language")]
        public LanguageType Language { get; set; }

        [JsonProperty("color")]
        public bool IsColor { get; set; }

        [JsonProperty("source")]
        public ExternalSource[] Sources { get; set; }
    }
}