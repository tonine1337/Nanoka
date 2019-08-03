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

        public void Apply(DoujinshiPage page)
        {
            Cid         = page.Cid ?? Cid;
            Index       = page.Index;
            Source      = page.Source ?? Source;
            Width       = page.Width;
            Height      = page.Height;
            SizeInBytes = page.SizeInBytes;
            MediaType   = page.MediaType ?? MediaType;
        }

        public void ApplyTo(DoujinshiPage page)
        {
            page.Cid         = Cid ?? page.Cid;
            page.Index       = Index;
            page.Source      = Source ?? page.Source;
            page.Width       = Width;
            page.Height      = Height;
            page.SizeInBytes = SizeInBytes;
            page.MediaType   = MediaType ?? page.MediaType;
        }
    }
}