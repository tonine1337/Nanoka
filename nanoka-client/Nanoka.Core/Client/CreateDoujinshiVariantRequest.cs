using System.ComponentModel.DataAnnotations;
using Nanoka.Core.Models;
using Newtonsoft.Json;

namespace Nanoka.Core.Client
{
    public class CreateDoujinshiVariantRequest
    {
        [JsonProperty("variant")]
        public DoujinshiVariantBase Variant { get; set; }

        [JsonProperty("pages"), Required, MinLength(1)]
        public DoujinshiPageBase[] Pages { get; set; }
    }
}