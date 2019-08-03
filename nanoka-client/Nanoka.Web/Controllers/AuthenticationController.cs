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
using Newtonsoft.Json;

namespace Nanoka.Web.Controllers
{
    [ApiController]
    [Route("users")]
    public class AuthenticationController : ControllerBase
    {
        readonly NanokaOptions _options;
        readonly NetDoujinshiDbContext _db;

        public AuthenticationController(IOptions<NanokaOptions> options, NetDoujinshiDbContext db)
        {
            _options = options.Value;
            _db      = db;
        }

        public class AuthRequest
        {
            [JsonProperty("token")]
            public Guid Token { get; set; }
        }

        public class AuthResponse
        {
            [JsonProperty("accessToken")]
            public string AccessToken { get; set; }

            [JsonProperty("user")]
            public object User { get; set; }

            [JsonProperty("expiry")]
            public DateTime Expiry { get; set; }
        }

        [HttpPost("auth")]
        public async Task<Result<AuthResponse>> AuthAsync(AuthRequest request)
        {
            var user = await _db.Users
                                .AsNoTracking()
                                .Include(u => u.Role)
                                .FirstOrDefaultAsync(u => u.Token == request.Token);

            if (user == null)
                return Result.StatusCode(HttpStatusCode.Unauthorized, $"Bad token '{request.Token}'.");

            var expiry  = DateTime.UtcNow.AddMinutes(30);
            var handler = new JwtSecurityTokenHandler();

            return new AuthResponse
            {
                AccessToken = handler.WriteToken(handler.CreateToken(new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, ((int) user.Role.Permissions).ToString())
                    }),
                    Expires = expiry,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.Default.GetBytes(_options.Secret)),
                        SecurityAlgorithms.HmacSha256Signature)
                })),

                User   = user.Convert(),
                Expiry = expiry
            };
        }
    }
}