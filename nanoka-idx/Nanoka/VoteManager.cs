using System;
using System.Threading;
using System.Threading.Tasks;
using Nanoka.Database;
using Nanoka.Models;

namespace Nanoka
{
    public class VoteManager
    {
        readonly INanokaDatabase _db;
        readonly UserClaimSet _claims;

        public VoteManager(INanokaDatabase db, UserClaimSet claims)
        {
            _db     = db;
            _claims = claims;
        }

        public async Task SetAsync<T>(T entity, VoteType? type, CancellationToken cancellationToken = default)
            where T : IHasId, IHasEntityType
        {
            if (type == null)
            {
                var vote = await _db.GetVoteAsync(_claims.Id, entity.Type, entity.Id, cancellationToken);

                if (vote == null)
                    return;

                await _db.DeleteVoteAsync(vote, cancellationToken);
            }
            else
            {
                var vote = new Vote
                {
                    UserId     = _claims.Id,
                    EntityType = entity.Type,
                    EntityId   = entity.Id,
                    Type       = type.Value,
                    Time       = DateTime.UtcNow
                };

                await _db.UpdateVoteAsync(vote, cancellationToken);
            }
        }

        public async Task DeleteAsync<T>(T entity, CancellationToken cancellationToken = default)
            where T : IHasId, IHasEntityType
            => await _db.DeleteVotesAsync(entity.Type, entity.Id, cancellationToken);
    }
}