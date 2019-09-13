using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka
{
    public class UserManager
    {
        readonly NanokaOptions _options;
        readonly INanokaDatabase _db;
        readonly ILocker _locker;
        readonly PasswordHashHelper _hash;
        readonly SnapshotManager _snapshot;
        readonly UserClaimSet _claims;

        public UserManager(IOptions<NanokaOptions> options, INanokaDatabase db, NamedLocker locker,
                           PasswordHashHelper hash, SnapshotManager snapshot, UserClaimSet claims)
        {
            _options  = options.Value;
            _db       = db;
            _locker   = locker.Get<UserManager>();
            _hash     = hash;
            _snapshot = snapshot;
            _claims   = claims;
        }

        public async Task CreateAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            using (await _locker.EnterAsync(username, cancellationToken))
            {
                // ensure username is unique
                if (await _db.GetUserByNameAsync(username, cancellationToken) != null)
                    throw Result.BadRequest($"Cannot use the username '{username}'.").Exception;

                var user = new User
                {
                    Username    = username,
                    Secret      = _hash.Hash(password),
                    Permissions = _options.DefaultUserPermissions
                };

                await _db.UpdateUserAsync(user, cancellationToken);
                await _snapshot.CreatedAsync(SnapshotType.System, user, cancellationToken, user.Id);
            }
        }

        public async Task<User> TryAuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            var user = await _db.GetUserByNameAsync(username, cancellationToken);

            return _hash.Test(password, user?.Secret)
                ? EraseConfidentialFields(user)
                : null;
        }

        User EraseConfidentialFields(User user)
        {
            // never return secret
            user.Secret = null;

            // only mods can see email
            if (!_claims.HasAnyPermission(UserPermissions.Moderator | UserPermissions.Administrator))
                user.Email = null;

            return user;
        }
    }
}