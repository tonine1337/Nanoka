using Nanoka.Core.Models;
using Nest;

namespace Nanoka.Web.Database
{
    [ElasticsearchType(RelationName = nameof(User), IdProperty = nameof(Id))]
    public class DbUser
    {
        public string Id { get; set; }

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
    }
}