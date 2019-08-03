using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class BooruImage
    {
        [JsonProperty("cid")]
        public string Cid { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("size")]
        public int SizeInBytes { get; set; }

        [JsonProperty("type")]
        public string MediaType { get; set; }
    }
}