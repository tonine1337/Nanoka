using System;
using Newtonsoft.Json;

namespace Nanoka.Core.Client
{
    public class AuthenticationRequest
    {
        [JsonProperty("token")]
        public Guid Token { get; set; }
    }

    public class AuthenticationResponse
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("user")]
        public object User { get; set; }

        [JsonProperty("expiry")]
        public DateTime Expiry { get; set; }
    }
}