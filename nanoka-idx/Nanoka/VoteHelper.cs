using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka
{
    public class VoteHelper
    {
        readonly IVoteRepository _votes;
        readonly IUserClaims _claims;
        readonly ILogger<VoteHelper> _logger;

        public VoteHelper(IVoteRepository votes, IUserClaims claims, ILogger<VoteHelper> logger)
        {
            _votes  = votes;
            _claims = claims;
            _logger = logger;
        }

        /// <returns>Null if nothing was affected. Otherwise a <see cref="Vote"/> object.</returns>
        public async Task<Vote> SetAsync<T>(T entity, VoteType? type, CancellationToken cancellationToken = default)
            where T : IHasId, IHasEntityType, IHasScore
        {
            // find existing vote
            var vote = await _votes.GetAsync(_claims.Id, entity.Type, entity.Id, cancellationToken);

            if (vote == null)
            {
                if (type == null)
                    return null; // no existing vote and not setting a vote

                vote = new Vote
                {
                    UserId     = _claims.Id,
                    EntityType = entity.Type,
                    EntityId   = entity.Id
                };
            }
            else
            {
                // offset previous weight
                entity.Score -= vote.Weight;

                // user requested vote be removed completely
                if (type == null)
                {
                    await _votes.DeleteAsync(vote, cancellationToken);

                    _logger.LogInformation("Unset vote of {0} score={1:F}", vote, entity.Score);

                    return vote;
                }
            }

            vote.Type = type.Value;
            vote.Time = DateTime.UtcNow;

            // calculate vote weight
            var userRep = _claims.Reputation;

            vote.Weight = 0.5 + Math.Clamp(userRep / 10, 1, 3) / 2;

            if (_claims.IsRestricted)
                vote.Weight *= 0.2;

            if (type.Value == VoteType.Down)
                vote.Weight = -vote.Weight;

            // offset with new weight
            entity.Score += vote.Weight;

            await _votes.UpdateAsync(vote, cancellationToken);

            _logger.LogInformation("Set vote of {0} score={1:F}", vote, entity.Score);

            return vote;
        }

        public async Task DeleteAsync<T>(T entity, CancellationToken cancellationToken = default)
            where T : IHasId, IHasEntityType
        {
            var deleted = await _votes.DeleteAsync(entity.Type, entity.Id, cancellationToken);

            _logger.LogInformation($"Deleted {deleted} votes of {entity.Type} {entity.Id}.");
        }
    }
}