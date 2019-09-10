using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models.Requests
{
    public class CreateBookVariantRequest
    {
        [JsonProperty("variant"), Required]
        public BookVariant Variant { get; set; }
    }
}