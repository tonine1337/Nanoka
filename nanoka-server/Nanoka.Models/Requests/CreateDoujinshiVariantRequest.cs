using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models.Requests
{
    public class CreateDoujinshiVariantRequest
    {
        [JsonProperty("variant"), Required]
        public DoujinshiVariant Variant { get; set; }
    }
}