using System.Collections.Generic;
using Nanoka.Core;
using Nanoka.Core.Models;
using Nest;

namespace Nanoka.Web.Database
{
    // nested object of doujinshi
    public class DbDoujinshiVariant
    {
        [Keyword(Name = "id", Index = false)]
        public string Id { get; set; }

        [Keyword(Name = "cid", Index = false)]
        public string Cid { get; set; }

        [Keyword(Name = "upu")]
        public string UploaderId { get; set; }

        [Text(Name = "a")]
        public string[] Artist { get; set; }

        [Text(Name = "g")]
        public string[] Group { get; set; }

        [Text(Name = "p")]
        public string[] Parody { get; set; }

        [Text(Name = "c")]
        public string[] Character { get; set; }

        [Text(Name = "l")]
        public string[] Language { get; set; }

        [Text(Name = "t")]
        public string[] Tag { get; set; }

        [Text(Name = "co")]
        public string[] Convention { get; set; }

        [Text(Name = "src")]
        public string Source { get; set; }

        [Number(Name = "pg")]
        public int PageCount { get; set; }

        public DbDoujinshiVariant Apply(DoujinshiVariant variant)
        {
            if (variant == null)
                return null;

            Id         = variant.Id.ToShortString();
            Cid        = variant.Cid ?? Cid;
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
            variant.Id         = Id.ToGuid();
            variant.Cid        = Cid ?? variant.Cid;
            variant.UploaderId = UploaderId.ToGuid();

            variant.Metas = new Dictionary<DoujinshiMeta, string[]>
            {
                { DoujinshiMeta.Artist, Artist ?? variant.Metas.GetOrDefault(DoujinshiMeta.Artist) },
                { DoujinshiMeta.Group, Group ?? variant.Metas.GetOrDefault(DoujinshiMeta.Group) },
                { DoujinshiMeta.Parody, Parody ?? variant.Metas.GetOrDefault(DoujinshiMeta.Parody) },
                { DoujinshiMeta.Character, Character ?? variant.Metas.GetOrDefault(DoujinshiMeta.Character) },
                { DoujinshiMeta.Language, Language ?? variant.Metas.GetOrDefault(DoujinshiMeta.Language) },
                { DoujinshiMeta.Tag, Tag ?? variant.Metas.GetOrDefault(DoujinshiMeta.Tag) },
                { DoujinshiMeta.Convention, Convention ?? variant.Metas.GetOrDefault(DoujinshiMeta.Convention) }
            };

            variant.Source    = Source ?? variant.Source;
            variant.PageCount = PageCount;

            return variant;
        }
    }
}
