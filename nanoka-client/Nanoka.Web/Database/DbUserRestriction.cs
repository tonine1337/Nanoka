using System;
using Nanoka.Core;
using Nanoka.Core.Models;
using Nest;

namespace Nanoka.Web.Database
{
    // nested object of user
    public class DbUserRestriction
    {
        [Date(Name = "s")]
        public DateTime Start { get; set; }

        [Date(Name = "e")]
        public DateTime End { get; set; }

        [Text(Name = "r")]
        public string Reason { get; set; }

        [Keyword(Name = "src", Index = false)]
        public string Source { get; set; }

        public DbUserRestriction Apply(UserRestriction restriction)
        {
            Start  = restriction.Start;
            End    = restriction.End;
            Reason = restriction.Reason ?? Reason;
            Source = restriction.Source.ToShortString();

            return this;
        }

        public UserRestriction ApplyTo(UserRestriction restriction)
        {
            restriction.Start  = Start;
            restriction.End    = End;
            restriction.Reason = Reason ?? restriction.Reason;
            restriction.Source = Source.ToGuid();

            return restriction;
        }
    }
}