using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Options;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka
{
    //todo: permission affecting actions should invalidate tokens (blacklist)
    public class UserManager
    {
        readonly NanokaOptions _options;
        readonly INanokaDatabase _db;
        readonly IMapper _mapper;
        readonly ILocker _locker;
        readonly PasswordHashHelper _hash;
        readonly SnapshotManager _snapshot;
        readonly TokenManager _token;
        readonly IUserClaims _claims;

        public UserManager(IOptions<NanokaOptions> options, INanokaDatabase db, ILocker locker, IMapper mapper,
                           PasswordHashHelper hash, SnapshotManager snapshot, TokenManager token, IUserClaims claims)
        {
            _options  = options.Value;
            _db       = db;
            _mapper   = mapper;
            _locker   = locker;
            _hash     = hash;
            _snapshot = snapshot;
            _token    = token;
            _claims   = claims;
        }

        User EraseConfidential(User user)
        {
            if (user == null)
                return null;

            // never return secret
            user.Secret = null;

            // only mods can see email
            if (!_claims.HasPermissions(UserPermissions.Moderator))
                user.Email = null;

            return user;
        }

        public async Task<User> CreateAsync(string username, string password, CancellationToken cancellationToken = default)
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

                return EraseConfidential(user);
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
            EnsureUserUpdatable(id);

            using (await _locker.EnterAsync(id, cancellationToken))
            {
                var user = await GetAsync(id, cancellationToken);

                _mapper.Map(model, user);

                await _db.UpdateUserAsync(user, cancellationToken);
                await _snapshot.ModifiedAsync(user, cancellationToken);

                return EraseConfidential(user);
            }
        }

        void EnsureUserUpdatable(string id)
        {
            // only allow users themselves or mods to update this user
            if (_claims.Id == id || _claims.HasPermissions(UserPermissions.Moderator))
                return;

            throw Result.Forbidden("Insufficient permissions to update this user.").Exception;
        }

        public async Task<Snapshot<User>[]> GetSnapshotsAsync(string id, CancellationToken cancellationToken = default)
            => (await _snapshot.GetAsync<User>(id, cancellationToken)).ToArray(s =>
            {
                s.Value = EraseConfidential(s.Value);
                return s;
            });

        public async Task<User> RevertAsync(string id, string snapshotId, CancellationToken cancellationToken = default)
        {
            EnsureUserUpdatable(id);

            using (await _locker.EnterAsync(id, cancellationToken))
            {
                var snapshot = await _snapshot.GetAsync<User>(snapshotId, id, cancellationToken);
                var user     = await _db.GetUserByIdAsync(id, cancellationToken);

                if (user != null && snapshot.Value == null)
                {
                    await _db.DeleteUserAsync(user, cancellationToken);

                    user = null;
                }
                else if (snapshot.Value != null)
                {
                    user = snapshot.Value;

                    await _db.UpdateUserAsync(user, cancellationToken);
                }

                await _snapshot.RevertedAsync(snapshot, cancellationToken);
                await _token.InvalidateAsync(id, cancellationToken);

                return EraseConfidential(user);
            }
        }

        public async Task<UserRestriction> AddRestrictionAsync(string id, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            if (duration < TimeSpan.FromMinutes(10))
                throw Result.BadRequest($"Invalid restriction duration '{duration}'. Duration must be at least 10 minutes.").Exception;

            EnsureUserUpdatable(id);

            using (await _locker.EnterAsync(id, cancellationToken))
            {
                var user = await GetAsync(id, cancellationToken);
                var time = DateTime.UtcNow;

                // if the user already has an active restriction, add to the duration
                var last = user.Restrictions?.LastOrDefault();

                if (last != null && time < last.End)
                    time = user.Restrictions.Last().End;

                // append to the list of restrictions
                var restriction = new UserRestriction
                {
                    Start       = time,
                    End         = time + duration,
                    ModeratorId = _claims.Id,
                    Reason      = _claims.GetReason()
                };

                user.Restrictions = (user.Restrictions ?? new UserRestriction[0]).Append(restriction).ToArray();

                await _db.UpdateUserAsync(user, cancellationToken);
                await _snapshot.ModifiedAsync(user, cancellationToken);
                await _token.InvalidateAsync(user.Id, cancellationToken);

                return restriction;
            }
        }

        public async Task<User> DerestrictAsync(string id, bool onlyActiveRestrictions = true, CancellationToken cancellationToken = default)
        {
            EnsureUserUpdatable(id);

            using (await _locker.EnterAsync(id, cancellationToken))
            {
                var user = await GetAsync(id, cancellationToken);
                var time = DateTime.UtcNow;

                if (user.Restrictions != null)
                {
                    var changed = false;
                    var list    = user.Restrictions.ToList();

                    foreach (var restriction in list)
                    {
                        // currently active restriction
                        if (restriction.Start <= time && time < restriction.End)
                        {
                            // move restriction end to current time, ending it immediately
                            restriction.End = time;

                            changed = true;
                        }

                        // future restriction
                        else if (!onlyActiveRestrictions && time < restriction.Start)
                        {
                            // remove future restrictions
                            list.Remove(restriction);

                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        user.Restrictions = list.ToArray();

                        await _db.UpdateUserAsync(user, cancellationToken);
                        await _snapshot.ModifiedAsync(user, cancellationToken);
                        await _token.InvalidateAsync(user.Id, cancellationToken);
                    }
                }

                return EraseConfidential(user);
            }
        }
    }
}