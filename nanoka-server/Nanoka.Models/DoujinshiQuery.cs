using System;
using System.Collections.Generic;
using Nanoka.Models.Query;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class DoujinshiQuery : QueryWrapperBase<DoujinshiQuery, DoujinshiQuerySort>
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
        public FilterQuery<DoujinshiCategory> Category { get; set; }

        [JsonProperty("score")]
        public RangeQuery<int> Score { get; set; }

        [JsonProperty("metas")]
        public Dictionary<DoujinshiMeta, TextQuery> Metas { get; set; }

        [JsonProperty("language")]
        public FilterQuery<LanguageType> Language { get; set; }

        [JsonProperty("source")]
        public TextQuery Source { get; set; }

        [JsonProperty("pages")]
        public RangeQuery<int> PageCount { get; set; }

        public DoujinshiQuery WithUploadTime(RangeQuery<DateTime> q) => Set(x => x.UploadTime = q);
        public DoujinshiQuery WithUpdateTime(RangeQuery<DateTime> q) => Set(x => x.UpdateTime = q);
        public DoujinshiQuery WithName(TextQuery q) => Set(x => x.Name = q);
        public DoujinshiQuery WithRomanizedName(TextQuery q) => Set(x => x.RomanizedName = q);
        public DoujinshiQuery WithCategory(FilterQuery<DoujinshiCategory> q) => Set(x => x.Category = q);
        public DoujinshiQuery WithScore(RangeQuery<int> q) => Set(x => x.Score = q);
        public DoujinshiQuery WithMeta(DoujinshiMeta meta, TextQuery q) => Set(x => x.Metas[meta] = q);
        public DoujinshiQuery WithLanguage(FilterQuery<LanguageType> q) => Set(x => x.Language = q);
        public DoujinshiQuery WithSource(TextQuery q) => Set(x => x.Source = q);
        public DoujinshiQuery WithPageCount(RangeQuery<int> q) => Set(x => x.PageCount = q);
    }
}