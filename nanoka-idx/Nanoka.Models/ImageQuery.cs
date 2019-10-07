using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class ImageQuery : QueryBase<ImageQuery>
    {
        [JsonProperty("score")]
        public RangeQuery<double> Score { get; set; }

        [JsonProperty("width")]
        public RangeQuery<int> Width { get; set; }

        [JsonProperty("height")]
        public RangeQuery<int> Height { get; set; }

        [JsonProperty("type")]
        public FilterQuery<ImageMediaType> MediaType { get; set; }

        [JsonProperty("tags")]
        public Dictionary<ImageTag, TextQuery> Tags { get; set; }

        [JsonProperty("source")]
        public FilterQuery<ExternalSource> Source { get; set; }

        [JsonProperty("rating")]
        public FilterQuery<MaterialRating> Rating { get; set; }

        [JsonProperty("noteCount")]
        public RangeQuery<int> NoteCount { get; set; }

        public ImageQuery WithScore(RangeQuery<double> q) => Set(x => x.Score = q);
        public ImageQuery WithWidth(RangeQuery<int> q) => Set(x => x.Width = q);
        public ImageQuery WithHeight(RangeQuery<int> q) => Set(x => x.Height = q);
        public ImageQuery WithTag(ImageTag tag, TextQuery q) => Set(x => x.Tags[tag] = q);
        public ImageQuery WithMediaType(FilterQuery<ImageMediaType> q) => Set(x => x.MediaType = q);
        public ImageQuery WithSource(FilterQuery<ExternalSource> q) => Set(x => x.Source = q);
        public ImageQuery WithRating(FilterQuery<MaterialRating> q) => Set(x => x.Rating = q);
        public ImageQuery WithNoteCount(RangeQuery<int> q) => Set(x => x.NoteCount = q);
    }
}