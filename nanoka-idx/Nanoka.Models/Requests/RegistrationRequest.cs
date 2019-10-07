using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Nanoka.Models.Requests
{
    public class RegistrationRequest
    {
        [JsonProperty("username"), Required, MaxLength(UserBase.UsernameMaxLength), RegularExpression(UserBase.UsernameRegex)]
        public string Username { get; set; }

        [JsonProperty("password"), Required, MaxLength(UserBase.PasswordMaxLength)]
        public string Password { get; set; }
    }

    public class RegistrationResponse
    {
        [JsonProperty("user")]
        public User User { get; set; }
    }
}