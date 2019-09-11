using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class ImageNote
    {
        [JsonProperty("x"), Range(0, short.MaxValue)]
        public int X { get; set; }

        [JsonProperty("y"), Range(0, short.MaxValue)]
        public int Y { get; set; }

        [JsonProperty("width"), Range(0, short.MaxValue)]
        public int Width { get; set; }

        [JsonProperty("height"), Range(0, short.MaxValue)]
        public int Height { get; set; }

        [JsonProperty("content"), Required, MaxLength(4096)]
        public string Content { get; set; }
    }
}