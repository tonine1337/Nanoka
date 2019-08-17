using Newtonsoft.Json;

namespace Nanoka.Models.Requests
{
    public class CreateDoujinshiVariantRequest
    {
        [JsonProperty("variant")]
        public DoujinshiVariant Variant { get; set; }
    }
}