using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka
{
    public class UserManager : IDisposable
    {
        readonly NanokaOptions _options;
        readonly INanokaDatabase _db;
        readonly NamedLockManager _lockManager;
        readonly PasswordHashHelper _hashHelper;
        readonly SnapshotManager _snapshotManager;

        public UserManager(IOptions<NanokaOptions> options, INanokaDatabase db, NamedLockManager lockManager,
                           PasswordHashHelper hashHelper, SnapshotManager snapshotManager)
        {
            _options         = options.Value;
            _db              = db;
            _lockManager     = lockManager;
            _hashHelper      = hashHelper;
            _snapshotManager = snapshotManager;
        }

        public async Task CreateAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            using (await _lockManager.EnterAsync(username, cancellationToken))
            {
                // ensure username is unique
                if (await _db.GetUserAsync(username, cancellationToken) != null)
                    throw new UserManagerException($"Cannot use the username '{username}'.");

                var user = new User
                {
                    Username    = username,
                    Secret      = _hashHelper.Hash(password),
                    Permissions = _options.DefaultUserPermissions
                };

                user.Id = await _db.UpdateUserAsync(user, cancellationToken);

                await _snapshotManager.UserCreatedAsync(user, cancellationToken);
            }
        }

        public async Task<User> TryAuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            var user = await _db.GetUserAsync(username, cancellationToken);

            return _hashHelper.Test(password, user?.Secret) ? user : null;
        }

        public void Dispose() => _lockManager.Dispose();
    }
}