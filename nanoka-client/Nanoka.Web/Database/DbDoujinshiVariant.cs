using System.Collections.Generic;
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
    }
}