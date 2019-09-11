using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nanoka.Models.Requests;

namespace Nanoka.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        readonly NanokaOptions _options;
        readonly UserManager _userManager;
        readonly RecaptchaValidator _recaptcha;

        public UserController(IOptions<NanokaOptions> options, UserManager userManager, RecaptchaValidator recaptcha)
        {
            _options     = options.Value;
            _userManager = userManager;
            _recaptcha   = recaptcha;
        }

        [HttpPost("auth")]
        public async Task<Result<AuthenticationResponse>> AuthAsync(AuthenticationRequest request)
        {
            var user = await _userManager.TryAuthenticateAsync(request.Username, request.Password);

            if (user == null)
                return Result.StatusCode(HttpStatusCode.Unauthorized, $"Invalid login for user {request.Username}.");

            var expiry  = DateTime.UtcNow.AddMinutes(30);
            var handler = new JwtSecurityTokenHandler();

            return new AuthenticationResponse
            {
                AccessToken = handler.WriteToken(handler.CreateToken(new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, ((int) user.Permissions).ToString()),
                        new Claim("rep", user.Reputation.ToString("F")),
                        new Claim("rest", user.Restrictions != null && user.Restrictions.Any(r => DateTime.UtcNow < r.End) ? "1" : "0")
                    }),
                    Expires = expiry,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.Default.GetBytes(_options.Secret)),
                        SecurityAlgorithms.HmacSha256Signature)
                })),

                User   = user,
                Expiry = expiry
            };
        }

        [HttpPost("register")]
        public async Task<Result<RegistrationResponse>> RegisterAsync(RegistrationRequest request, [FromQuery] string recaptcha)
        {
            if (!await _recaptcha.ValidateAsync(recaptcha))
                return Result.InvalidRecaptchaToken(recaptcha);

            await _userManager.CreateAsync(request.Username, request.Password);

            return new RegistrationResponse();
        }
    }
}