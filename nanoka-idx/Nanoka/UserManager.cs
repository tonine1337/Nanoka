using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Options;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka
{
    public class UserManager
    {
        readonly NanokaOptions _options;
        readonly INanokaDatabase _db;
        readonly IMapper _mapper;
        readonly ILocker _locker;
        readonly PasswordHashHelper _hash;
        readonly SnapshotManager _snapshot;
        readonly IUserClaims _claims;

        public UserManager(IOptions<NanokaOptions> options, INanokaDatabase db, NamedLocker locker, IMapper mapper,
                           PasswordHashHelper hash, SnapshotManager snapshot, IUserClaims claims)
        {
            _options  = options.Value;
            _db       = db;
            _mapper   = mapper;
            _locker   = locker.Get<UserManager>();
            _hash     = hash;
            _snapshot = snapshot;
            _claims   = claims;
        }

        User EraseConfidential(User user)
        {
            // never return secret
            user.Secret = null;

            // only mods can see email
            if (!_claims.HasPermissions(UserPermissions.Moderator))
                user.Email = null;

            return user;
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
                await _snapshot.CreatedAsync(user, cancellationToken, SnapshotType.System, user.Id);
            }
        }

        public async Task<User> TryAuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            var user = await _db.GetUserByNameAsync(username, cancellationToken);

            return _hash.Test(password, user?.Secret)
                ? EraseConfidential(user)
                : null;
        }

        public async Task<User> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            var user = await _db.GetUserByIdAsync(id, cancellationToken);

            if (user == null)
                throw Result.NotFound<User>(id).Exception;

            return EraseConfidential(user);
        }

        public async Task<User> UpdateAsync(string id, UserBase model, CancellationToken cancellationToken = default)
        {
            using (await _locker.EnterAsync(id, cancellationToken))
            {
                var user = await GetAsync(id, cancellationToken);

                // only allow users themselves or mods to update users
                if (!(_claims.Id == user.Id || _claims.HasPermissions(UserPermissions.Moderator)))
                    throw Result.Forbidden("Insufficient permissions to update this user.").Exception;

                _mapper.Map(model, user);

                await _db.UpdateUserAsync(user, cancellationToken);
                await _snapshot.ModifiedAsync(user, cancellationToken);

                return EraseConfidential(user);
            }
        }
    }
}