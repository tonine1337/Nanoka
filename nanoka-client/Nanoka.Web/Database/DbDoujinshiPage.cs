using Nanoka.Core.Models;
using Nest;

namespace Nanoka.Web.Database
{
    // nested object of doujinshi variant
    public class DbDoujinshiPage
    {
        [Text(Name = "cid")]
        public string Cid { get; set; }

        [Number(Name = "i")]
        public int Index { get; set; }

        [Text(Name = "src")]
        public string Source { get; set; }

        [Number(Name = "w")]
        public int Width { get; set; }

        [Number(Name = "h")]
        public int Height { get; set; }

        [Number(Name = "s")]
        public int SizeInBytes { get; set; }

        [Text(Name = "t")]
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
