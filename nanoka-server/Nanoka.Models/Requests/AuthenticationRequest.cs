using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models.Requests
{
    public class AuthenticationRequest
    {
        [JsonProperty("id"), Required]
        public Guid Id { get; set; }

        [JsonProperty("secret"), Required]
        public Guid Secret { get; set; }
    }

    public class AuthenticationResponse
    {
        [JsonProperty("version")]
        public NanokaVersion Version { get; set; } = NanokaVersion.Latest;

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