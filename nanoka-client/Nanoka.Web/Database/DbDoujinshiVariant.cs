using System.Collections.Generic;
using Nanoka.Core;
using Nanoka.Core.Models;
using Nest;

namespace Nanoka.Web.Database
{
    // nested object of doujinshi
    public class DbDoujinshiVariant
    {
        [Text(Name = "a")]
        public string[] Artist { get; set; }

        [Text(Name = "g")]
        public string[] Group { get; set; }

        [Text(Name = "p")]
        public string[] Parody { get; set; }

        [Text(Name = "c")]
        public string[] Character { get; set; }

        [Text(Name = "ca")]
        public string[] Category { get; set; }

        [Text(Name = "l")]
        public string[] Language { get; set; }

        [Text(Name = "t")]
        public string[] Tag { get; set; }

        [Text(Name = "co")]
        public string[] Convention { get; set; }

        [Text(Name = "src")]
        public string Source { get; set; }

        [Nested(Name = "pages")]
        public List<DbDoujinshiPage> Pages { get; set; }

        public void Apply(DoujinshiVariant variant)
        {
            Artist     = variant.Metas?.GetOrDefault(DoujinshiMeta.Artist) ?? Artist;
            Group      = variant.Metas?.GetOrDefault(DoujinshiMeta.Group) ?? Group;
            Parody     = variant.Metas?.GetOrDefault(DoujinshiMeta.Parody) ?? Parody;
            Character  = variant.Metas?.GetOrDefault(DoujinshiMeta.Character) ?? Character;
            Category   = variant.Metas?.GetOrDefault(DoujinshiMeta.Category) ?? Category;
            Language   = variant.Metas?.GetOrDefault(DoujinshiMeta.Language) ?? Language;
            Tag        = variant.Metas?.GetOrDefault(DoujinshiMeta.Tag) ?? Tag;
            Convention = variant.Metas?.GetOrDefault(DoujinshiMeta.Convention) ?? Convention;
            Source     = variant.Source ?? Source;

            Pages = variant.Pages?.ToList(p =>
            {
                var page = new DbDoujinshiPage();
                page.Apply(p);

                return page;
            }) ?? Pages;
        }

        public void ApplyTo(DoujinshiVariant variant)
        {
            variant.Metas = Extensions.BuildArrayDict(
                                (DoujinshiMeta.Artist, Artist),
                                (DoujinshiMeta.Group, Group),
                                (DoujinshiMeta.Parody, Parody),
                                (DoujinshiMeta.Character, Character),
                                (DoujinshiMeta.Category, Category),
                                (DoujinshiMeta.Language, Language),
                                (DoujinshiMeta.Tag, Tag),
                                (DoujinshiMeta.Convention, Convention)) ?? variant.Metas;

            variant.Source = Source ?? variant.Source;

            variant.Pages = Pages?.ToArray(p =>
            {
                var page = new DoujinshiPage();
                p.ApplyTo(page);

                return page;
            }) ?? variant.Pages;
        }
    }
}