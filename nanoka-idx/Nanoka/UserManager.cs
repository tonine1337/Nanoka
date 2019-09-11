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
        readonly PasswordHashHelper _hashHelper;

        public UserManager(IOptions<NanokaOptions> options, INanokaDatabase db, PasswordHashHelper hashHelper)
        {
            _options    = options.Value;
            _db         = db;
            _hashHelper = hashHelper;
        }

        public async Task CreateAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            var user = new User
            {
                Username    = username,
                Secret      = _hashHelper.Hash(password),
                Permissions = _options.DefaultUserPermissions
            };

            await _db.UpdateUserAsync(user, cancellationToken);
        }

        public async Task<User> TryAuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            var user = await _db.GetUserAsync(username, cancellationToken);

            return _hashHelper.Test(password, user?.Secret) ? user : null;
        }
    }
}