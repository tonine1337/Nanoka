using System.Collections.Generic;
using Nanoka.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Database
{
    // nested object of doujinshi
    public class DbDoujinshiVariant
    {
        /// <summary>
        /// This is not a true ID.
        /// It is only used to discriminate the variants in the same doujinshi.
        /// It is named "Id2" because NEST will infer "Id" to be _id.
        /// </summary>
        [Keyword(Name = "id", Index = false), JsonProperty("id")]
        public string Id2 { get; set; }

        [Keyword(Name = "upu"), JsonProperty("upu")]
        public string UploaderId { get; set; }

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

        [Text(Name = "src"), JsonProperty("src")]
        public string Source { get; set; }

        [Number(Name = "pg", Index = false), JsonProperty("pg")] // cached in DbDoujinshi
        public int PageCount { get; set; }

        public DbDoujinshiVariant Apply(DoujinshiVariant variant)
        {
            if (variant == null)
                return null;

            Id2        = variant.Id.ToShortString();
            UploaderId = variant.UploaderId.ToShortString();

            Artist     = variant.Metas?.GetOrDefault(DoujinshiMeta.Artist) ?? Artist;
            Group      = variant.Metas?.GetOrDefault(DoujinshiMeta.Group) ?? Group;
            Parody     = variant.Metas?.GetOrDefault(DoujinshiMeta.Parody) ?? Parody;
            Character  = variant.Metas?.GetOrDefault(DoujinshiMeta.Character) ?? Character;
            Language   = variant.Metas?.GetOrDefault(DoujinshiMeta.Language) ?? Language;
            Tag        = variant.Metas?.GetOrDefault(DoujinshiMeta.Tag) ?? Tag;
            Convention = variant.Metas?.GetOrDefault(DoujinshiMeta.Convention) ?? Convention;

            Source    = variant.Source ?? Source;
            PageCount = variant.PageCount;

            return this;
        }

        public DoujinshiVariant ApplyTo(DoujinshiVariant variant)
        {
            variant.Id         = Id2.ToGuid();
            variant.UploaderId = UploaderId.ToGuid();

            variant.Metas = new Dictionary<DoujinshiMeta, string[]>
            {
                { DoujinshiMeta.Artist, Artist ?? variant.Metas?.GetOrDefault(DoujinshiMeta.Artist) },
                { DoujinshiMeta.Group, Group ?? variant.Metas?.GetOrDefault(DoujinshiMeta.Group) },
                { DoujinshiMeta.Parody, Parody ?? variant.Metas?.GetOrDefault(DoujinshiMeta.Parody) },
                { DoujinshiMeta.Character, Character ?? variant.Metas?.GetOrDefault(DoujinshiMeta.Character) },
                { DoujinshiMeta.Language, Language ?? variant.Metas?.GetOrDefault(DoujinshiMeta.Language) },
                { DoujinshiMeta.Tag, Tag ?? variant.Metas?.GetOrDefault(DoujinshiMeta.Tag) },
                { DoujinshiMeta.Convention, Convention ?? variant.Metas?.GetOrDefault(DoujinshiMeta.Convention) }
            };

            variant.Source    = Source ?? variant.Source;
            variant.PageCount = PageCount;

            return variant;
        }
    }
}