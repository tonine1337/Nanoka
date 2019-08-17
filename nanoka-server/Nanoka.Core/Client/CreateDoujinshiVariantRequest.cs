using System.ComponentModel.DataAnnotations;
using Nanoka.Core.Models;
using Newtonsoft.Json;

namespace Nanoka.Core.Client
{
    public class CreateDoujinshiVariantRequest
    {
        [JsonProperty("variant"), Required]
        public DoujinshiVariantBase Variant { get; set; }

        [JsonProperty("cid"), Required]
        public string Cid { get; set; }
    }
}