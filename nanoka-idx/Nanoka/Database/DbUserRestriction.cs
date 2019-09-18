using System;
using Nanoka.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Database
{
    // nested object of DbUser
    public class DbUserRestriction
    {
        [Date(Name = "s", Index = false), JsonProperty("s")]
        public DateTime Start { get; set; }

        [Date(Name = "e", Index = false), JsonProperty("e")]
        public DateTime End { get; set; }

        [Keyword(Name = "m", Index = false), JsonProperty("m")]
        public string ModeratorId { get; set; }

        [Text(Name = "r", Index = false), JsonProperty("r")]
        public string Reason { get; set; }

        public UserRestriction ToRestriction() => new UserRestriction
        {
            Start       = Start,
            End         = End,
            ModeratorId = ModeratorId,
            Reason      = Reason
        };

        public static DbUserRestriction FromRestriction(UserRestriction restriction) => new DbUserRestriction
        {
            Start       = restriction.Start,
            End         = restriction.End,
            ModeratorId = restriction.ModeratorId,
            Reason      = restriction.Reason
        };
    }
}
