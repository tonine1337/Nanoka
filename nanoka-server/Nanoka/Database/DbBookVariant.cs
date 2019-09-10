using Nanoka.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Database
{
    // nested object of book
    public class DbBookVariant
    {
        /// <summary>
        /// This is not a true ID.
        /// It is only used to discriminate the variants in the same book.
        /// It is named "Id2" because NEST will infer "Id" to be _id.
        /// </summary>
        [Keyword(Name = "id", Index = false), JsonProperty("id")]
        public string Id2 { get; set; }

        [Keyword(Name = "upu"), JsonProperty("upu")]
        public string UploaderId { get; set; }

        [Text(Name = "n", Index = false), JsonProperty("n")] // cached in DbBook
        public string Name { get; set; }

        [Text(Name = "nr", Index = false), JsonProperty("nr")] // cached in DbBook
        public string RomanizedName { get; set; }

        [Number(NumberType.Integer, Name = "ln"), JsonProperty("ln")]
        public LanguageType Language { get; set; }

        [Text(Name = "src"), JsonProperty("src")]
        public string Source { get; set; }

        [Number(NumberType.Integer, Name = "pg", Index = false), JsonProperty("pg")] // cached in DbBook
        public int PageCount { get; set; }

        public DbBookVariant Apply(BookVariant variant)
        {
            if (variant == null)
                return null;

            Id2           = variant.Id.ToShortString();
            UploaderId    = variant.UploaderId.ToShortString();
            Name          = variant.Name ?? Name;
            RomanizedName = variant.RomanizedName ?? RomanizedName;
            Language      = variant.Language;
            Source        = variant.Source ?? Source;
            PageCount     = variant.PageCount;

            return this;
        }

        public BookVariant ApplyTo(BookVariant variant)
        {
            variant.Id            = Id2.ToGuid();
            variant.UploaderId    = UploaderId.ToGuid();
            variant.Name          = Name ?? variant.Name;
            variant.RomanizedName = RomanizedName ?? variant.RomanizedName;
            variant.Language      = Language;
            variant.Source        = Source ?? variant.Source;
            variant.PageCount     = PageCount;

            return variant;
        }
    }
}