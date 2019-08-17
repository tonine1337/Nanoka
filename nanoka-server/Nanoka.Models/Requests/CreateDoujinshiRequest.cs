using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models.Requests
{
    public class CreateDoujinshiRequest : CreateDoujinshiVariantRequest
    {
        [JsonProperty("doujinshi"), Required]
        public DoujinshiBase Doujinshi { get; set; }
    }
}