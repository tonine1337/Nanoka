using System;
using System.Collections.Generic;
using Nanoka.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Database
{
    [ElasticsearchType(RelationName = nameof(Doujinshi), IdProperty = nameof(Id))]
    public class DbDoujinshi
    {
        [Keyword(Name = "id", Index = false), JsonProperty("id")]
        public string Id { get; set; }

        [Date(Name = "up"), JsonProperty("up")]
        public DateTime UploadTime { get; set; }

        [Date(Name = "ud"), JsonProperty("ud")]
        public DateTime UpdateTime { get; set; }

#region Metas

        [Text(Name = "a"), JsonProperty("a")]
        public string[] Artist { get; set; }

        [Text(Name = "g"), JsonProperty("g")]
        public string[] Group { get; set; }

        [Text(Name = "p"), JsonProperty("p")]
        public string[] Parody { get; set; }

        [Text(Name = "c"), JsonProperty("c")]
        public string[] Character { get; set; }

        [Text(Name = "l"), JsonProperty("l")]
        public string[] Language { get; set; }

        [Text(Name = "t"), JsonProperty("t")]
        public string[] Tag { get; set; }

        [Text(Name = "co"), JsonProperty("co")]
        public string[] Convention { get; set; }

#endregion

        [Number(NumberType.Integer, Name = "ca"), JsonProperty("ca")]
        public DoujinshiCategory Category { get; set; }

        [Number(NumberType.Integer, Name = "sc"), JsonProperty("sc")]
        public int Score { get; set; }

        [Nested(Name = "var"), JsonProperty("var")]
        public List<DbDoujinshiVariant> Variants { get; set; }

#region Cached

        // these values are cached to allow fast searching without querying nested objects

        [PropertyName("n"), JsonIgnore]
        public string[] Names { get; set; }

        [PropertyName("nr"), JsonIgnore]
        public string[] RomanizedNames { get; set; }

        [Number(NumberType.Integer, Name = "pg_n"), JsonIgnore]
        public int[] PageCounts { get; set; }

#endregion

        public DbDoujinshi Apply(Doujinshi doujinshi)
        {
            if (doujinshi == null)
                return null;

            Id         = doujinshi.Id.ToShortString();
            UploadTime = doujinshi.UploadTime;
            UpdateTime = doujinshi.UpdateTime;

            Artist     = doujinshi.Metas?.GetValueOrDefault(DoujinshiMeta.Artist) ?? Artist;
            Group      = doujinshi.Metas?.GetValueOrDefault(DoujinshiMeta.Group) ?? Group;
            Parody     = doujinshi.Metas?.GetValueOrDefault(DoujinshiMeta.Parody) ?? Parody;
            Character  = doujinshi.Metas?.GetValueOrDefault(DoujinshiMeta.Character) ?? Character;
            Tag        = doujinshi.Metas?.GetValueOrDefault(DoujinshiMeta.Tag) ?? Tag;
            Convention = doujinshi.Metas?.GetValueOrDefault(DoujinshiMeta.Convention) ?? Convention;

            Category = doujinshi.Category;
            Score    = doujinshi.Score;

            Variants = doujinshi.Variants?.ToList(v => new DbDoujinshiVariant().Apply(v)) ?? Variants;

            Names          = Variants.ToArray(v => v.Name);
            RomanizedNames = Variants.ToArray(v => v.RomanizedName);
            PageCounts     = Variants.ToArray(v => v.PageCount);

            return this;
        }

        public Doujinshi ApplyTo(Doujinshi doujinshi)
        {
            doujinshi.Id         = Id.ToGuid();
            doujinshi.UploadTime = UploadTime;
            doujinshi.UpdateTime = UpdateTime;

            doujinshi.Metas = new Dictionary<DoujinshiMeta, string[]>
            {
                { DoujinshiMeta.Artist, Artist ?? doujinshi.Metas?.GetValueOrDefault(DoujinshiMeta.Artist) },
                { DoujinshiMeta.Group, Group ?? doujinshi.Metas?.GetValueOrDefault(DoujinshiMeta.Group) },
                { DoujinshiMeta.Parody, Parody ?? doujinshi.Metas?.GetValueOrDefault(DoujinshiMeta.Parody) },
                { DoujinshiMeta.Character, Character ?? doujinshi.Metas?.GetValueOrDefault(DoujinshiMeta.Character) },
                { DoujinshiMeta.Tag, Tag ?? doujinshi.Metas?.GetValueOrDefault(DoujinshiMeta.Tag) },
                { DoujinshiMeta.Convention, Convention ?? doujinshi.Metas?.GetValueOrDefault(DoujinshiMeta.Convention) }
            };

            doujinshi.Category = Category;
            doujinshi.Score    = Score;

            doujinshi.Variants = Variants?.ToList(v => v.ApplyTo(new DoujinshiVariant())) ?? doujinshi.Variants;

            return doujinshi;
        }
    }
}