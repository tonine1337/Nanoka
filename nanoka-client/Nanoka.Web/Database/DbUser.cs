using Nanoka.Core.Models;
using Nest;

namespace Nanoka.Web.Database
{
    [ElasticsearchType(RelationName = nameof(User), IdProperty = nameof(Id))]
    public class DbUser
    {
        public string Id { get; set; }

        [Text(Name = "sec")]
        public string Secret { get; set; }

        [Text(Name = "n")]
        public string Username { get; set; }

        [Text(Name = "res")]
        public bool IsRestricted { get; set; }

        [Nested(Name = "sc_dj")]
        public DbUserScores DoujinshiScores { get; set; }

        [Nested(Name = "sc_bo")]
        public DbUserScores BooruScores { get; set; }

        [Number(Name = "rep")]
        public double Reputation { get; set; }

        public DbUser Apply(User user)
        {
            if (user == null)
                return null;

            Id           = user.Id.ToShortString();
            Secret       = user.Secret.ToShortString();
            Username     = user.Username ?? Username;
            IsRestricted = user.IsRestricted;

            DoujinshiScores = new DbUserScores().Apply(user.DoujinshiScores) ?? DoujinshiScores;
            BooruScores     = new DbUserScores().Apply(user.BooruScores) ?? BooruScores;

            Reputation = user.Reputation;

            return this;
        }

        public User ApplyTo(User user)
        {
            if (user == null)
                return null;

            user.Id           = Id.ToGuid();
            user.Secret       = Secret.ToGuid();
            user.Username     = Username ?? user.Username;
            user.IsRestricted = IsRestricted;

            user.DoujinshiScores = DoujinshiScores?.ApplyTo(new UserScores()) ?? user.DoujinshiScores;
            user.BooruScores     = BooruScores?.ApplyTo(new UserScores()) ?? user.BooruScores;

            user.Reputation = Reputation;

            return user;
        }
    }
}