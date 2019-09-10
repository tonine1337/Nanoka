using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    /// <summary>
    /// Represents a single article in the wiki.
    /// </summary>
    public class Article : ArticleBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class ArticleBase
    {
        [JsonProperty("title"), Required]
        public string Title { get; set; }

        [JsonProperty("content"), Required]
        public string Content { get; set; }
    }
}