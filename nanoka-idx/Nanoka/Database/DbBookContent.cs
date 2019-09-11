using Nanoka.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Database
{
    // nested object of DbBook
    // fields are not indexed here
    public class DbBookContent
    {
        // used to distinguish different book contents
        [Keyword(Name = "d", Index = false), JsonProperty("id")]
        public int Discriminator { get; set; }

        [Number(NumberType.Integer, Name = "pc", Index = false), JsonProperty("pc")]
        public int PageCount { get; set; }

        [Keyword(Name = "l", Index = false), JsonProperty("l")]
        public LanguageType Language { get; set; }

        [Boolean(Name = "col", Index = false), JsonProperty("col")]
        public bool IsColor { get; set; }

        [Text(Name = "src", Index = false), JsonProperty("src")]
        public string[] Sources { get; set; }

        public BookContent ToContent() => new BookContent
        {
            Id        = Discriminator,
            PageCount = PageCount,
            Language  = Language,
            IsColor   = IsColor,
            Sources   = Sources.ToArray(ExternalSource.Parse)
        };

        public static DbBookContent FromContent(BookContent content) => new DbBookContent
        {
            Discriminator = content.Id,
            PageCount     = content.PageCount,
            Language      = content.Language,
            IsColor       = content.IsColor,
            Sources       = content.Sources.ToArray(s => s.ToString())
        };
    }
}