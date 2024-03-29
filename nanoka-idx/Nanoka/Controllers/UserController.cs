using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nanoka.Database;
using Nanoka.Models;
using Nanoka.Models.Requests;
using Swashbuckle.AspNetCore.Annotations;

namespace Nanoka.Controllers
{
    [ApiController, Route("users"), Authorize]
    public class UserController : ControllerBase
    {
        readonly NanokaOptions _options;
        readonly IUserRepository _users;
        readonly ILocker _locker;
        readonly IMapper _mapper;
        readonly IUserClaims _claims;
        readonly PasswordHashHelper _hash;
        readonly SnapshotHelper _snapshot;
        readonly TokenManager _tokens;

        public UserController(IOptions<NanokaOptions> options, IUserRepository users, ILocker locker, IMapper mapper,
                              IUserClaims claims, PasswordHashHelper hash, SnapshotHelper snapshot, TokenManager tokens)
        {
            _options  = options.Value;
            _users    = users;
            _locker   = locker;
            _mapper   = mapper;
            _claims   = claims;
            _hash     = hash;
            _snapshot = snapshot;
            _tokens   = tokens;
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

        bool IsUserUpdatable(string id, out ActionResult result)
        {
            // only allow users themselves or mods to update this user
            if (_claims.Id == id || _claims.HasPermissions(UserPermissions.Moderator))
            {
                result = null;
                return true;
            }

            result = StatusCode((int) HttpStatusCode.Forbidden, "Insufficient permissions to update this user.");
            return false;
        }

        /// <summary>
        /// Authenticates using username and password.
        /// </summary>
        /// <param name="request">Authentication request.</param>
        [HttpPost("auth")]
        [AllowAnonymous, SwaggerOperation(OperationId = "authenticateUser")]
        public async Task<ActionResult<AuthenticationResponse>> AuthenticateAsync(AuthenticationRequest request)
        {
            var user = await _users.GetByNameAsync(request.Username);

            if (!_hash.Test(request.Password, user?.Secret))
                return Unauthorized($"Invalid login for user '{request.Username}'.");

            // access token can live extremely long since we have an on-demand invalidation mechanism
            var expiry = DateTime.UtcNow.AddMonths(1);

            return new AuthenticationResponse
            {
                AccessToken = await _tokens.GenerateTokenAsync(user, expiry),
                User        = EraseConfidential(user),
                Expiry      = expiry
            };
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="request">Creation request.</param>
        [HttpPost("register")]
        [AllowAnonymous, VerifyHuman, SwaggerOperation(OperationId = "createUser")]
        public async Task<ActionResult<RegistrationResponse>> RegisterAsync(RegistrationRequest request)
        {
            using (await _locker.EnterAsync(request.Username))
            {
                // ensure username is unique
                if (await _users.GetByNameAsync(request.Username) != null)
                    return BadRequest($"Cannot use the username '{request.Username}'.");

                var user = new User
                {
                    Username    = request.Username,
                    Secret      = _hash.Hash(request.Password),
                    Permissions = _options.DefaultUserPermissions
                };

                await _users.UpdateAsync(user);
                await _snapshot.CreatedAsync(user, default, SnapshotType.System, user.Id);

                return new RegistrationResponse
                {
                    User = EraseConfidential(user)
                };
            }
        }

        /// <summary>
        /// Retrieves user information.
        /// </summary>
        /// <param name="id">User ID.</param>
        [HttpGet("{id}")]
        [SwaggerOperation(OperationId = "getUser")]
        public async Task<ActionResult<User>> GetAsync(string id)
        {
            var user = await _users.GetByIdAsync(id);

            if (user == null)
                return ResultUtilities.NotFound<User>(id);

            return EraseConfidential(user);
        }

        /// <summary>
        /// Updates user information.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="model">New user information.</param>
        [HttpPut("{id}")]
        [UserClaims(Unrestricted = true), SwaggerOperation(OperationId = "updateUser")]
        public async Task<ActionResult<User>> UpdateAsync(string id, UserBase model)
        {
            if (!IsUserUpdatable(id, out var result))
                return result;

            using (await _locker.EnterAsync(id))
            {
                var user = await _users.GetByIdAsync(id);

                if (user == null)
                    return ResultUtilities.NotFound<User>(id);

                _mapper.Map(model, user);

                await _users.UpdateAsync(user);
                await _snapshot.ModifiedAsync(user);

                return EraseConfidential(user);
            }
        }

        /// <summary>
        /// Retrieves snapshots of user information.
        /// </summary>
        /// <param name="id">User ID.</param>
        [HttpGet("{id}/snapshots")]
        [SwaggerOperation(OperationId = "getUserSnapshots")]
        public async Task<Snapshot<User>[]> GetSnapshotsAsync(string id)
            => (await _snapshot.GetAsync<User>(id)).ToArray(s =>
            {
                s.Value = EraseConfidential(s.Value);
                return s;
            });

        /// <summary>
        /// Adds a restriction to a user.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="request">Restriction request.</param>
        [HttpPost("{id}/restrictions")]
        [UserClaims(Unrestricted = true, Permissions = UserPermissions.Moderator, Reason = true), SwaggerOperation(OperationId = "addUserRestriction")]
        public async Task<ActionResult<UserRestriction>> AddRestrictionAsync(string id, RestrictUserRequest request)
        {
            if (request.Duration < TimeSpan.FromMinutes(10))
                return BadRequest($"Invalid restriction duration '{request.Duration}'. Duration must be at least 10 minutes.");

            if (!IsUserUpdatable(id, out var result))
                return result;

            using (await _locker.EnterAsync(id))
            {
                var user = await _users.GetByIdAsync(id);

                if (user == null)
                    return ResultUtilities.NotFound<User>(id);

                var time = DateTime.UtcNow;

                // if the user already has an active restriction, add to the duration
                var last = user.Restrictions?.LastOrDefault();

                if (last != null && time < last.End)
                    time = user.Restrictions.Last().End;

                // append to the list of restrictions
                var restriction = new UserRestriction
                {
                    Start       = time,
                    End         = time + request.Duration,
                    ModeratorId = _claims.Id,
                    Reason      = _claims.GetReason()
                };

                user.Restrictions = (user.Restrictions ?? new UserRestriction[0]).Append(restriction).ToArray();

                await _users.UpdateAsync(user);
                await _snapshot.ModifiedAsync(user);
                await _tokens.InvalidateAsync(user.Id);

                return restriction;
            }
        }

        /// <summary>
        /// Ends all currently active restrictions for a user.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="all">Whether to remove future restrictions</param>
        [HttpDelete("{id}/restrictions")]
        [UserClaims(Unrestricted = true, Permissions = UserPermissions.Moderator, Reason = true), SwaggerOperation(OperationId = "clearUserRestrictions")]
        public async Task<ActionResult<User>> DerestrictAsync(string id, [FromQuery] bool all)
        {
            if (!IsUserUpdatable(id, out var result))
                return result;

            using (await _locker.EnterAsync(id))
            {
                var user = await _users.GetByIdAsync(id);

                if (user == null)
                    return ResultUtilities.NotFound<User>(id);

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
                        else if (all && time < restriction.Start)
                        {
                            // remove future restrictions
                            list.Remove(restriction);

                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        user.Restrictions = list.ToArray();

                        await _users.UpdateAsync(user);
                        await _snapshot.ModifiedAsync(user);
                        await _tokens.InvalidateAsync(user.Id);
                    }
                }

                return EraseConfidential(user);
            }
        }
    }
}