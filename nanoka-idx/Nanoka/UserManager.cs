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

        public UserManager(IOptions<NanokaOptions> options, INanokaDatabase db, NamedLocker locker,
                           PasswordHashHelper hash, SnapshotManager snapshot)
        {
            _options  = options.Value;
            _db       = db;
            _locker   = locker.Get<UserManager>();
            _hash     = hash;
            _snapshot = snapshot;
        }

        public async Task CreateAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            using (await _locker.EnterAsync(username, cancellationToken))
            {
                // ensure username is unique
                if (await _db.GetUserAsync(username, cancellationToken) != null)
                    throw new UserManagerException($"Cannot use the username '{username}'.");

                var user = new User
                {
                    Username    = username,
                    Secret      = _hash.Hash(password),
                    Permissions = _options.DefaultUserPermissions
                };

                user.Id = await _db.UpdateUserAsync(user, cancellationToken);

                await _snapshot.UserCreated(user, cancellationToken);
            }
        }

        public async Task<User> TryAuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            var user = await _db.GetUserAsync(username, cancellationToken);

            return _hash.Test(password, user?.Secret) ? user : null;
        }
    }
}