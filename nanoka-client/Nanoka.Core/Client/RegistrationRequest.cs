using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Core.Client
{
    public class RegistrationRequest
    {
        [JsonProperty("username"), Required, MaxLength(20)]
        [RegularExpression(@"^(?=.{4,20}$)(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$")]
        public string Username { get; set; }

        [JsonProperty("recaptcha"), Required]
        public string RecaptchaToken { get; set; }
    }

    public class RegistrationResponse
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("secret")]
        public Guid Secret { get; set; }
    }
}