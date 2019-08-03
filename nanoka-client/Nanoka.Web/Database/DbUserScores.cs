using Nanoka.Core.Models;
using Nest;

namespace Nanoka.Web.Database
{
    // nested object of user
    public class DbUserScores
    {
        [Number(Name = "up")]
        public int UploadCount { get; set; }

        [Number(Name = "up_a")]
        public int UploadAcceptedCount { get; set; }

        [Number(Name = "ed")]
        public int EditCount { get; set; }

        [Number(Name = "ed_a")]
        public int EditAcceptedCount { get; set; }

        [Number(Name = "vu")]
        public int UpvotedCount { get; set; }

        [Number(Name = "vd")]
        public int DownvotedCount { get; set; }

        public DbUserScores Apply(UserScores scores)
        {
            if (scores == null)
                return null;

            UploadCount         = scores.UploadCount;
            UploadAcceptedCount = scores.UploadAcceptedCount;
            EditCount           = scores.EditCount;
            EditAcceptedCount   = scores.EditAcceptedCount;
            UpvotedCount        = scores.UpvotedCount;
            DownvotedCount      = scores.DownvotedCount;

            return this;
        }

        public UserScores ApplyTo(UserScores scores)
        {
            if (scores == null)
                return null;

            scores.UploadCount         = UploadCount;
            scores.UploadAcceptedCount = UploadAcceptedCount;
            scores.EditCount           = EditCount;
            scores.EditAcceptedCount   = EditAcceptedCount;
            scores.UpvotedCount        = UpvotedCount;
            scores.DownvotedCount      = DownvotedCount;

            return scores;
        }
    }
}