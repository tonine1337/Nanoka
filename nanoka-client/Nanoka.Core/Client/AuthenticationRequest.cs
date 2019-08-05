using System;
using Nanoka.Core.Models;
using Newtonsoft.Json;

namespace Nanoka.Core.Client
{
    public class AuthenticationRequest
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("secret")]
        public Guid Secret { get; set; }
    }

    public class AuthenticationResponse
    {
        /// <summary>
        /// JWT bearer token.
        /// </summary>
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Expiry time of <see cref="AccessToken"/> in UTC.
        /// </summary>
        [JsonProperty("expiry")]
        public DateTime Expiry { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }
    }
}