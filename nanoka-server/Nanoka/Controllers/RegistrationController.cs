using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nanoka.Database;
using Nanoka.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nanoka.Controllers
{
    [ApiController]
    [Route("users")]
    public class RegistrationController : ControllerBase
    {
        readonly NanokaOptions _options;
        readonly HttpClient _http;
        readonly NanokaDatabase _db;

        public RegistrationController(IOptions<NanokaOptions> options,
                                      IHttpClientFactory httpClientFactory,
                                      NanokaDatabase db)
        {
            _options = options.Value;
            _http    = httpClientFactory.CreateClient(nameof(RegistrationController));
            _db      = db;
        }

        public class RegistrationRequest
        {
            [JsonProperty("username"), Required]
            [RegularExpression(UserBase.UsernameRegex)]
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

        [HttpPost("register")]
        public async Task<Result<RegistrationResponse>> RegisterAsync(RegistrationRequest request)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "secret", _options.RecaptchaSecret },
                { "response", request.RecaptchaToken }
            });

            var res = await _http.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

            if (res.StatusCode != HttpStatusCode.OK)
                return Result.BadRequest("Failed reCAPTCHA.");

            dynamic jsonData = JObject.Parse(await res.Content.ReadAsStringAsync());

            if (!(bool) jsonData.success)
                return Result.BadRequest("Failed reCAPTCHA.");

            // create user with random ID and secret
            var user = new User
            {
                Id          = Guid.NewGuid(),
                Secret      = Extensions.SecureGuid(),
                Username    = request.Username,
                Registered  = DateTime.UtcNow,
                Permissions = _options.DefaultUserPermissions
            };

            // save to database
            await _db.IndexAsync(user);

            return new RegistrationResponse
            {
                Id     = user.Id,
                Secret = user.Secret
            };
        }
    }
}