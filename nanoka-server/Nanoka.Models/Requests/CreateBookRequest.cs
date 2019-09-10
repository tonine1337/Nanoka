using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models.Requests
{
    public class CreateBookRequest : CreateBookVariantRequest
    {
        [JsonProperty("book"), Required]
        public BookBase Book { get; set; }
    }
}