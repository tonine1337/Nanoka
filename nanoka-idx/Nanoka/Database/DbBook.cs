using System.Collections.Generic;
using System.Linq;
using Nanoka.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Database
{
    [ElasticsearchType(RelationName = nameof(Book))]
    public class DbBook : IHasId
    {
        [Keyword(Name = "id", Index = false), JsonProperty("id")]
        public string Id { get; set; }

        [Text(Name = "n"), JsonProperty("n")]
        public string[] Name { get; set; }

        [Number(NumberType.Double, Name = "s"), JsonProperty("s")]
        public double Score { get; set; }

        [Text(Name = "tg"), JsonProperty("tg")]
        public string[] TagsGeneral { get; set; }

        [Text(Name = "ta"), JsonProperty("ta")]
        public string[] TagsArtist { get; set; }

        [Text(Name = "tp"), JsonProperty("tp")]
        public string[] TagsParody { get; set; }

        [Text(Name = "tc"), JsonProperty("tc")]
        public string[] TagsCharacter { get; set; }

        [Text(Name = "tco"), JsonProperty("tco")]
        public string[] TagsConvention { get; set; }

        [Text(Name = "ts"), JsonProperty("ts")]
        public string[] TagsSeries { get; set; }

        [Keyword(Name = "c"), JsonProperty("c")]
        public BookCategory Category { get; set; }

        [Keyword(Name = "r"), JsonProperty("r")]
        public MaterialRating Rating { get; set; }

        [Nested(Name = "cont"), JsonProperty("cont")]
        public DbBookContent[] Contents { get; set; }

#region Cached

        [Number(NumberType.Integer, Name = "pc"), JsonIgnore]
        public int[] PageCounts { get; set; }

        [Keyword(Name = "l"), JsonIgnore]
        public LanguageType[] Languages { get; set; }

        [Boolean(Name = "col"), JsonIgnore]
        public bool IsColor { get; set; }

        [Keyword(Name = "src"), JsonIgnore]
        public string[] Sources { get; set; }

#endregion

        public Book ToBook() => new Book
        {
            Id    = Id,
            Name  = Name,
            Score = Score,
            Tags = new Dictionary<BookTag, string[]>
            {
                { BookTag.General, TagsGeneral },
                { BookTag.Artist, TagsArtist },
                { BookTag.Parody, TagsParody },
                { BookTag.Character, TagsCharacter },
                { BookTag.Convention, TagsConvention },
                { BookTag.Series, TagsSeries }
            }.RemoveNullValues(),
            Category = Category,
            Rating   = Rating,
            Contents = Contents.ToArray(c => c.ToContent())
        };

        public static DbBook FromBook(Book book)
        {
            var contents = book.Contents.ToArray(DbBookContent.FromContent);

            return new DbBook
            {
                Id             = book.Id,
                Name           = book.Name,
                Score          = book.Score,
                TagsGeneral    = book.Tags.GetValueOrDefault(BookTag.General),
                TagsArtist     = book.Tags.GetValueOrDefault(BookTag.Artist),
                TagsParody     = book.Tags.GetValueOrDefault(BookTag.Parody),
                TagsCharacter  = book.Tags.GetValueOrDefault(BookTag.Character),
                TagsConvention = book.Tags.GetValueOrDefault(BookTag.Convention),
                TagsSeries     = book.Tags.GetValueOrDefault(BookTag.Series),
                Category       = book.Category,
                Rating         = book.Rating,
                Contents       = contents,
                PageCounts     = contents.ToArray(c => c.PageCount),
                Languages      = contents.Select(c => c.Language).ToHashSet().ToArray(),
                IsColor        = contents.Any(c => c.IsColor),
                Sources        = contents.SelectMany(c => c.Sources).ToHashSet().ToArray()
            };
        }
    }
}
