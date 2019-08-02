using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class DoujinshiPage
    {
        /// <summary>
        /// The CID of the image file of this page.
        /// </summary>
        [JsonProperty("cid")]
        public string Cid { get; set; }

        [JsonProperty("id")]
        public int Index { get; set; }

        [JsonProperty("src")]
        public string Source { get; set; }

        [JsonProperty("w")]
        public int Width { get; set; }

        [JsonProperty("h")]
        public int Height { get; set; }

        [JsonProperty("s")]
        public int SizeInBytes { get; set; }
    }
}
