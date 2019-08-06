using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nanoka.Core;
using Nanoka.Core.Client;
using Nanoka.Web.Database;

namespace Nanoka.Web.Controllers
{
    [ApiController]
    [Route("users")]
    public class AuthenticationController : ControllerBase
    {
        readonly NanokaOptions _options;
        readonly NanokaDatabase _db;

        public AuthenticationController(IOptions<NanokaOptions> options, NanokaDatabase db)
        {
            _options = options.Value;
            _db      = db;
        }

        [HttpPost("auth")]
        public async Task<Result<AuthenticationResponse>> AuthAsync(AuthenticationRequest request)
        {
            var user = await _db.GetUserAsync(request.Id);

            if (user == null || user.Secret != request.Secret)
                return Result.StatusCode(HttpStatusCode.Unauthorized, $"Invalid login for user {request.Id}.");

            var expiry  = DateTime.UtcNow.AddMinutes(30);
            var handler = new JwtSecurityTokenHandler();

            return new AuthenticationResponse
            {
                AccessToken = handler.WriteToken(handler.CreateToken(new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToShortString()),
                        new Claim(ClaimTypes.Role, ((int) user.Permissions).ToString())
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
    }
}