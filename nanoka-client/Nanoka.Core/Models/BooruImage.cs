using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class BooruImage
    {
        [JsonProperty("cid")]
        public string Cid { get; set; }

        [JsonProperty("src")]
        public string Source { get; set; }

        [JsonProperty("w")]
        public int Width { get; set; }

        [JsonProperty("h")]
        public int Height { get; set; }

        [JsonProperty("s")]
        public int SizeInBytes { get; set; }

        [JsonProperty("t")]
        public string MediaType { get; set; }
    }
}
