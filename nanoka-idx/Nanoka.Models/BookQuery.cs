using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class BookQuery : QueryBase<BookQuery, BookSort>
    {
        [JsonProperty("name")]
        public TextQuery Name { get; set; }

        [JsonProperty("score")]
        public RangeQuery<double> Score { get; set; }

        [JsonProperty("tags")]
        public Dictionary<BookTag, TextQuery> Tags { get; set; }

        [JsonProperty("category")]
        public FilterQuery<BookCategory> Category { get; set; }

        [JsonProperty("rating")]
        public FilterQuery<MaterialRating> Rating { get; set; }

        [JsonProperty("pageCount")]
        public RangeQuery<int> PageCount { get; set; }

        [JsonProperty("language")]
        public FilterQuery<LanguageType> Language { get; set; }

        [JsonProperty("color")]
        public FilterQuery<bool> IsColor { get; set; }

        [JsonProperty("source")]
        public FilterQuery<ExternalSource> Source { get; set; }

        public BookQuery WithName(TextQuery q) => Set(x => x.Name = q);
        public BookQuery WithScore(RangeQuery<double> q) => Set(x => x.Score = q);
        public BookQuery WithTag(BookTag tag, TextQuery q) => Set(x => x.Tags[tag] = q);
        public BookQuery WithCategory(FilterQuery<BookCategory> q) => Set(x => x.Category = q);
        public BookQuery WithRating(FilterQuery<MaterialRating> q) => Set(x => x.Rating = q);
        public BookQuery WithPageCount(RangeQuery<int> q) => Set(x => x.PageCount = q);
        public BookQuery WithLanguage(FilterQuery<LanguageType> q) => Set(x => x.Language = q);
        public BookQuery WithIsColor(FilterQuery<bool> q) => Set(x => x.IsColor = q);
        public BookQuery WithSource(FilterQuery<ExternalSource> q) => Set(x => x.Source = q);
    }
}