using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class ExternalSource
    {
        [JsonProperty("website"), Required]
        public string Website { get; set; }

        [JsonProperty("identifier"), Required]
        public string Identifier { get; set; }
    }
}