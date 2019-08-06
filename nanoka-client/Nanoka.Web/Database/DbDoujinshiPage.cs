using Nanoka.Core.Models;
using Nest;

namespace Nanoka.Web.Database
{
    // nested object of doujinshi variant
    public class DbDoujinshiPage
    {
        [Keyword(Name = "cid", Index = false)]
        public string Cid { get; set; }

        [Number(Name = "i", Index = false)]
        public int Index { get; set; }

        [Number(Name = "w", Index = false)]
        public int Width { get; set; }

        [Number(Name = "h", Index = false)]
        public int Height { get; set; }

        [Number(Name = "s", Index = false)]
        public int SizeInBytes { get; set; }

        [Keyword(Name = "t", Index = false)]
        public string MediaType { get; set; }

        public DbDoujinshiPage Apply(DoujinshiPage page)
        {
            if (page == null)
                return null;

            Cid         = page.Cid ?? Cid;
            Index       = page.Index;
            Width       = page.Width;
            Height      = page.Height;
            SizeInBytes = page.SizeInBytes;
            MediaType   = page.MediaType ?? MediaType;

            return this;
        }

        public DoujinshiPage ApplyTo(DoujinshiPage page)
        {
            page.Cid         = Cid ?? page.Cid;
            page.Index       = Index;
            page.Width       = Width;
            page.Height      = Height;
            page.SizeInBytes = SizeInBytes;
            page.MediaType   = MediaType ?? page.MediaType;

            return page;
        }
    }
}