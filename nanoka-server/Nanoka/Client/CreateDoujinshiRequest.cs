using System.ComponentModel.DataAnnotations;
using Nanoka.Models;
using Newtonsoft.Json;

namespace Nanoka.Client
{
    public class CreateDoujinshiRequest : CreateDoujinshiVariantRequest
    {
        [JsonProperty("doujinshi"), Required]
        public DoujinshiBase Doujinshi { get; set; }
    }
}