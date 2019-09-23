using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka
{
    public class TokenManager
    {
        readonly NanokaOptions _options;
        readonly INanokaDatabase _db;
        readonly UserManager _users;
        readonly IMemoryCache _cache;

        public TokenManager(IOptions<NanokaOptions> options, INanokaDatabase db, UserManager users, IMemoryCache cache)
        {
            _options = options.Value;
            _db      = db;
            _users   = users;
            _cache   = cache;
        }

        public string GenerateAccessToken(User user, DateTime expiry)
        {
            var handler = new JwtSecurityTokenHandler();

            return handler.WriteToken(handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Role, ((int) user.Permissions).ToString()),
                    new Claim("rep", user.Reputation.ToString("F")),
                    new Claim("rest", user.Restrictions != null && user.Restrictions.Any(r => DateTime.UtcNow < r.End) ? "1" : "0")
                }),
                Expires = expiry,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.Default.GetBytes(_options.Secret)),
                    SecurityAlgorithms.HmacSha256Signature)
            }));
        }
    }
}