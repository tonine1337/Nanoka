using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class ImageNote
    {
        /// <summary>
        /// X position in pixels from the top-left.
        /// </summary>
        [JsonProperty("x"), Range(0, short.MaxValue)]
        public int X { get; set; }

        /// <summary>
        /// Y position in pixels from the top-left.
        /// </summary>
        [JsonProperty("y"), Range(0, short.MaxValue)]
        public int Y { get; set; }

        /// <summary>
        /// Width in pixels.
        /// </summary>
        [JsonProperty("width"), Range(0, short.MaxValue)]
        public int Width { get; set; }

        /// <summary>
        /// Height in pixels.
        /// </summary>
        [JsonProperty("height"), Range(0, short.MaxValue)]
        public int Height { get; set; }

        /// <summary>
        /// Content text that supports markdown.
        /// </summary>
        [JsonProperty("content"), Required, MaxLength(4096)]
        public string Content { get; set; }
    }
}