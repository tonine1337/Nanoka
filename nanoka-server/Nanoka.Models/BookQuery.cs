using System;
using System.Collections.Generic;
using Nanoka.Models.Query;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class BookQuery : QueryWrapperBase<BookQuery, BookQuerySort>
    {
        [JsonProperty("upload")]
        public RangeQuery<DateTime> UploadTime { get; set; }

        [JsonProperty("update")]
        public RangeQuery<DateTime> UpdateTime { get; set; }

        [JsonProperty("name")]
        public TextQuery Name { get; set; }

        [JsonProperty("name_romanized")]
        public TextQuery RomanizedName { get; set; }

        [JsonProperty("category")]
        public FilterQuery<BookCategory> Category { get; set; }

        [JsonProperty("score")]
        public RangeQuery<int> Score { get; set; }

        [JsonProperty("metas")]
        public Dictionary<BookMeta, TextQuery> Metas { get; set; }

        [JsonProperty("language")]
        public FilterQuery<LanguageType> Language { get; set; }

        [JsonProperty("source")]
        public TextQuery Source { get; set; }

        [JsonProperty("pages")]
        public RangeQuery<int> PageCount { get; set; }

        public BookQuery WithUploadTime(RangeQuery<DateTime> q) => Set(x => x.UploadTime = q);
        public BookQuery WithUpdateTime(RangeQuery<DateTime> q) => Set(x => x.UpdateTime = q);
        public BookQuery WithName(TextQuery q) => Set(x => x.Name = q);
        public BookQuery WithRomanizedName(TextQuery q) => Set(x => x.RomanizedName = q);
        public BookQuery WithCategory(FilterQuery<BookCategory> q) => Set(x => x.Category = q);
        public BookQuery WithScore(RangeQuery<int> q) => Set(x => x.Score = q);
        public BookQuery WithMeta(BookMeta meta, TextQuery q) => Set(x => x.Metas[meta] = q);
        public BookQuery WithLanguage(FilterQuery<LanguageType> q) => Set(x => x.Language = q);
        public BookQuery WithSource(TextQuery q) => Set(x => x.Source = q);
        public BookQuery WithPageCount(RangeQuery<int> q) => Set(x => x.PageCount = q);
    }
}