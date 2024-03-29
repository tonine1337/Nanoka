using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Nanoka
{
    public class RecaptchaValidator
    {
        readonly RecaptchaOptions _options;
        readonly HttpClient _http;
        readonly ILogger<RecaptchaValidator> _logger;

        readonly bool _enabled;

        public RecaptchaValidator(IOptions<RecaptchaOptions> options,
                                  IHttpClientFactory httpClientFactory,
                                  ILogger<RecaptchaValidator> logger)
        {
            _options = options.Value;
            _http    = httpClientFactory.CreateClient(nameof(RecaptchaValidator));
            _logger  = logger;

            _enabled = _options.SiteKey != null && _options.SecretKey != null;

            if (!_enabled)
                logger.LogWarning("reCAPTCHA verification is disabled.");
        }

        public async Task<bool> TryValidateAsync(string token, CancellationToken cancellationToken = default)
        {
            if (!_enabled)
                return true;

            if (string.IsNullOrWhiteSpace(token))
                return false;

            var success = true;

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "secret", _options.SecretKey },
                { "response", token }
            });

            var response = await _http.PostAsync("https://www.google.com/recaptcha/api/siteverify", content, cancellationToken);

            success &= response.StatusCode == HttpStatusCode.OK;

            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());

            success &= (bool) result.success;

            if (!success)
            {
                _logger.LogDebug($"Failed reCAPTCHA verification for token '{token}'. {response.ReasonPhrase}");
                return false;
            }

            return true;
        }
    }
}