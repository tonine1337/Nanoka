using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nanoka.Database;
using Nanoka.Models;
using Nanoka.Models.Requests;

namespace Nanoka.Controllers
{
    [ApiController]
    [Route("users")]
    public class RegistrationController : ControllerBase
    {
        readonly NanokaOptions _options;
        readonly NanokaDatabase _db;
        readonly RecaptchaValidator _recaptcha;

        public RegistrationController(IOptions<NanokaOptions> options,
                                      NanokaDatabase db,
                                      RecaptchaValidator recaptcha)
        {
            _options   = options.Value;
            _db        = db;
            _recaptcha = recaptcha;
        }

        [HttpPost("register")]
        public async Task<Result<RegistrationResponse>> RegisterAsync(RegistrationRequest request, [FromQuery] string token)
        {
            if (!await _recaptcha.ValidateAsync(token))
                return Result.BadRequest("Failed reCAPTCHA verification.");

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