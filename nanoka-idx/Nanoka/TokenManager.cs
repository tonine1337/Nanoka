using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nanoka.Models;

namespace Nanoka
{
    public class TokenManager
    {
        readonly NanokaOptions _options;
        readonly IDistributedCache _cache;
        readonly IUserClaims _claims;

        public TokenManager(IOptions<NanokaOptions> options, IDistributedCache cache, IUserClaims claims)
        {
            _options = options.Value;
            _cache   = cache;
            _claims  = claims;
        }

        /// <summary>
        /// Versioning is used to invalidate all tokens on demand.
        /// </summary>
        async Task<int> GetUserVersionAsync(string userId, CancellationToken cancellationToken = default)
        {
            var buffer = await _cache.GetAsync(GetVersionKey(userId), cancellationToken);

            return buffer == null
                ? 0
                : BitConverter.ToInt32(buffer);
        }

        public async Task<string> GenerateTokenAsync(User user, DateTime expiry, CancellationToken cancellationToken = default)
        {
            var handler = new JwtSecurityTokenHandler();

            return handler.WriteToken(handler.CreateJwtSecurityToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("sub", user.Id),
                    new Claim("role", ((int) user.Permissions).ToString()),
                    new Claim("jti", (await GetUserVersionAsync(user.Id, cancellationToken)).ToString()),
                    new Claim("rep", user.Reputation.ToString("F")),
                    new Claim("rest", user.Restrictions != null && user.Restrictions.Any(r => DateTime.UtcNow < r.End) ? "1" : "0")
                }),
                Expires = expiry,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.Default.GetBytes(_options.Secret)),
                    SecurityAlgorithms.HmacSha256Signature)
            }));
        }

        public async Task<bool> IsValidAsync(CancellationToken cancellationToken = default)
        {
            // anonymous endpoint
            if (_claims.Id == null)
                return true;

            return _claims.Version == await GetUserVersionAsync(_claims.Id, cancellationToken);
        }

        public async Task InvalidateAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (_claims.Id == null)
                return;

            // increment version to invalidate all tokens generated using the previous version
            var buffer = BitConverter.GetBytes(_claims.Version + 1);

            await _cache.SetAsync(GetVersionKey(userId), buffer, cancellationToken);
        }

        static string GetVersionKey(string userId) => $"user:{userId}:version";
    }
}