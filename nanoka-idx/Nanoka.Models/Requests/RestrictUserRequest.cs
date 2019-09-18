using System;
using Newtonsoft.Json;

namespace Nanoka.Models.Requests
{
    public class RestrictUserRequest
    {
        [JsonProperty("duration")]
        public TimeSpan Duration { get; set; }
    }
}