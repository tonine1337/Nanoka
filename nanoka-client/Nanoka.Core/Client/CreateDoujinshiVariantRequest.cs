using Nanoka.Core.Models;
using Newtonsoft.Json;

namespace Nanoka.Core.Client
{
    public class CreateDoujinshiVariantRequest
    {
        [JsonProperty("variant")]
        public DoujinshiVariantBase Variant { get; set; }
    }
}