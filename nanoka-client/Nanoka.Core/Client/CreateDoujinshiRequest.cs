using System.ComponentModel.DataAnnotations;
using Nanoka.Core.Models;
using Newtonsoft.Json;

namespace Nanoka.Core.Client
{
    public class CreateDoujinshiRequest
    {
        [JsonProperty("doujinshi"), Required]
        public DoujinshiBase Doujinshi { get; set; }

        [JsonProperty("variant"), Required]
        public DoujinshiVariantBase Variant { get; set; }

        [JsonProperty("pages"), Required, MinLength(1)]
        public DoujinshiPageBase[] Pages { get; set; }
    }
}