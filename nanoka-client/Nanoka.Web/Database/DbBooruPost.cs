using System;
using Nanoka.Core.Models;
using Nest;

namespace Nanoka.Web.Database
{
    [ElasticsearchType(RelationName = nameof(BooruPost), IdProperty = nameof(Id))]
    public class DbBooruPost
    {
        public string Id { get; set; }

        [Date(Name = "up")]
        public DateTime UploadTime { get; set; }

        [Date(Name = "ud")]
        public DateTime UpdateTime { get; set; }

        [Text(Name = "a")]
        public string[] Artist { get; set; }

        [Text(Name = "c")]
        public string[] Character { get; set; }

        [Text(Name = "cp")]
        public string[] Copyright { get; set; }

        [Text(Name = "m")]
        public string[] Metadata { get; set; }

        [Text(Name = "g")]
        public string[] General { get; set; }

        [Number(Name = "ra")]
        public BooruRating Rating { get; set; }

        [Number(Name = "sc")]
        public int Score { get; set; }

        [Text(Name = "src")]
        public string Source { get; set; }

        [Number(Name = "w")]
        public int Width { get; set; }

        [Number(Name = "h")]
        public int Height { get; set; }

        [Number(Name = "s")]
        public int SizeInBytes { get; set; }

        [Text(Name = "t")]
        public string MediaType { get; set; }

        [Text(Name = "sib")]
        public string[] SiblingIds { get; set; }
    }
}