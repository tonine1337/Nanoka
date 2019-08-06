using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class DoujinshiPage : DoujinshiPageBase
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("size")]
        public int SizeInBytes { get; set; }

        [JsonProperty("type")]
        public string MediaType { get; set; }
    }

    public class DoujinshiPageBase
    {
        [JsonProperty("cid")]
        public string Cid { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }
    }
}