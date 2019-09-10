using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class SongLyrics
    {
        [JsonProperty("lines"), Required, MinLength(1)]
        public Dictionary<double, string> Lines { get; set; }
    }
}