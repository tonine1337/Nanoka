using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Models;
using Nanoka.Models.Requests;

namespace Nanoka.Controllers
{
    [ApiController]
    [Route("users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        readonly UserManager _userManager;
        readonly TokenManager _tokenManager;

        public UserController(UserManager userManager, TokenManager tokenManager)
        {
            _userManager  = userManager;
            _tokenManager = tokenManager;
        }

        [HttpPost("auth")]
        [AllowAnonymous]
        public async Task<Result<AuthenticationResponse>> AuthAsync(AuthenticationRequest request)
        {
            var user = await _userManager.TryAuthenticateAsync(request.Username, request.Password);

            if (user == null)
                return Result.StatusCode(HttpStatusCode.Unauthorized, $"Invalid login for user {request.Username}.");

            // access token can live extremely long since we have a blacklist
            var expiry = DateTime.UtcNow.AddMonths(1);

            return new AuthenticationResponse
            {
                AccessToken = _tokenManager.GenerateAccessToken(user, expiry),
                User        = user,
                Expiry      = expiry
            };
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [VerifyHuman]
        public async Task<Result<RegistrationResponse>> RegisterAsync(RegistrationRequest request)
        {
            await _userManager.CreateAsync(request.Username, request.Password);

            return new RegistrationResponse();
        }

        [HttpGet("{id}")]
        public async Task<Result<User>> GetAsync(string id)
            => await _userManager.GetAsync(id);

        [HttpPut("{id}")]
        [UserClaims(unrestricted: true)]
        public async Task<Result<User>> UpdateAsync(string id, UserBase user)
            => await _userManager.UpdateAsync(id, user);

        [HttpGet("{id}/snapshots")]
        public async Task<Result<Snapshot<User>[]>> GetSnapshotsAsync(string id)
            => await _userManager.GetSnapshotsAsync(id);

        [HttpPost("{id}/snapshots/revert")]
        [UserClaims(unrestricted: true, reason: true)]
        public async Task<Result<User>> RevertAsync(string id, RevertEntityRequest request)
            => await _userManager.RevertAsync(id, request.SnapshotId);

        [HttpPost("{id}/restrictions")]
        [UserClaims(unrestricted: true, permissions: UserPermissions.Moderator, reason: true)]
        public async Task<Result<UserRestriction>> AddRestrictionAsync(string id, RestrictUserRequest request)
            => await _userManager.AddRestrictionAsync(id, request.Duration);

        [HttpDelete("{id}/restrictions")]
        [UserClaims(unrestricted: true, permissions: UserPermissions.Moderator, reason: true)]
        public async Task<Result<User>> DerestrictAsync(string id)
            => await _userManager.DerestrictAsync(id);
    }
}