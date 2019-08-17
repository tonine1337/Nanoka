using System.ComponentModel.DataAnnotations;
using Nanoka.Models;
using Newtonsoft.Json;

namespace Nanoka.Client
{
    public class CreateDoujinshiVariantRequest
    {
        [JsonProperty("variant"), Required]
        public DoujinshiVariantBase Variant { get; set; }

        [JsonProperty("cid"), Required]
        public string Cid { get; set; }
    }
}