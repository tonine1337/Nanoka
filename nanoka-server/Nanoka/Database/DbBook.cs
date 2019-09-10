using System;
using System.Collections.Generic;
using Nanoka.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Database
{
    [ElasticsearchType(RelationName = nameof(Book), IdProperty = nameof(Id))]
    public class DbBook
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
        public BookCategory Category { get; set; }

        [Number(NumberType.Integer, Name = "sc"), JsonProperty("sc")]
        public int Score { get; set; }

        [Nested(Name = "var"), JsonProperty("var")]
        public List<DbBookVariant> Variants { get; set; }

#region Cached

        // these values are cached to allow fast searching without querying nested objects

        [PropertyName("n"), JsonIgnore]
        public string[] Names { get; set; }

        [PropertyName("nr"), JsonIgnore]
        public string[] RomanizedNames { get; set; }

        [Number(NumberType.Integer, Name = "pg_n"), JsonIgnore]
        public int[] PageCounts { get; set; }

#endregion

        public DbBook Apply(Book book)
        {
            if (book == null)
                return null;

            Id         = book.Id.ToShortString();
            UploadTime = book.UploadTime;
            UpdateTime = book.UpdateTime;

            Artist     = book.Metas?.GetValueOrDefault(BookMeta.Artist) ?? Artist;
            Group      = book.Metas?.GetValueOrDefault(BookMeta.Group) ?? Group;
            Parody     = book.Metas?.GetValueOrDefault(BookMeta.Parody) ?? Parody;
            Character  = book.Metas?.GetValueOrDefault(BookMeta.Character) ?? Character;
            Tag        = book.Metas?.GetValueOrDefault(BookMeta.Tag) ?? Tag;
            Convention = book.Metas?.GetValueOrDefault(BookMeta.Convention) ?? Convention;

            Category = book.Category;
            Score    = book.Score;

            Variants = book.Variants?.ToList(v => new DbBookVariant().Apply(v)) ?? Variants;

            Names          = Variants.ToArray(v => v.Name);
            RomanizedNames = Variants.ToArray(v => v.RomanizedName);
            PageCounts     = Variants.ToArray(v => v.PageCount);

            return this;
        }

        public Book ApplyTo(Book book)
        {
            book.Id         = Id.ToGuid();
            book.UploadTime = UploadTime;
            book.UpdateTime = UpdateTime;

            book.Metas = new Dictionary<BookMeta, string[]>
            {
                { BookMeta.Artist, Artist ?? book.Metas?.GetValueOrDefault(BookMeta.Artist) },
                { BookMeta.Group, Group ?? book.Metas?.GetValueOrDefault(BookMeta.Group) },
                { BookMeta.Parody, Parody ?? book.Metas?.GetValueOrDefault(BookMeta.Parody) },
                { BookMeta.Character, Character ?? book.Metas?.GetValueOrDefault(BookMeta.Character) },
                { BookMeta.Tag, Tag ?? book.Metas?.GetValueOrDefault(BookMeta.Tag) },
                { BookMeta.Convention, Convention ?? book.Metas?.GetValueOrDefault(BookMeta.Convention) }
            };

            book.Category = Category;
            book.Score    = Score;

            book.Variants = Variants?.ToList(v => v.ApplyTo(new BookVariant())) ?? book.Variants;

            return book;
        }
    }
}