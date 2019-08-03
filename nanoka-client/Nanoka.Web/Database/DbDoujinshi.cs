using System;
using System.Collections.Generic;
using Nanoka.Core.Models;
using Nest;

namespace Nanoka.Web.Database
{
    [ElasticsearchType(RelationName = nameof(Doujinshi), IdProperty = nameof(Id))]
    public class DbDoujinshi
    {
        public string Id { get; set; }

        [Date(Name = "up")]
        public DateTime UploadTime { get; set; }

        [Date(Name = "ud")]
        public DateTime UpdateTime { get; set; }

        [Text(Name = "on")]
        public string OriginalName { get; set; }

        [Text(Name = "rn")]
        public string RomanizedName { get; set; }

        [Text(Name = "en")]
        public string EnglishName { get; set; }

        [Number(Name = "sc")]
        public int Score { get; set; }

        [Nested(Name = "var")]
        public List<DbDoujinshiVariant> Variations { get; set; }
    }
}