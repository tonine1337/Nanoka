using System.ComponentModel.DataAnnotations;
using Nanoka.Core.Models;
using Newtonsoft.Json;

namespace Nanoka.Core.Client
{
    public class CreateDoujinshiRequest : CreateDoujinshiVariantRequest
    {
        [JsonProperty("doujinshi"), Required]
        public DoujinshiBase Doujinshi { get; set; }
    }
}