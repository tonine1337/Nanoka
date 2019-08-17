using Newtonsoft.Json;

namespace Nanoka.Models.Requests
{
    public class CreateDoujinshiRequest : CreateDoujinshiVariantRequest
    {
        [JsonProperty("doujinshi")]
        public DoujinshiBase Doujinshi { get; set; }
    }
}