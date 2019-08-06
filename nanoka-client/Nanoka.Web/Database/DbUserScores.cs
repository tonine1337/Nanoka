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
        public int UploadApprovedCount { get; set; }

        [Number(Name = "ed")]
        public int EditCount { get; set; }

        [Number(Name = "ed_a")]
        public int EditApprovedCount { get; set; }

        [Number(Name = "vu")]
        public int UpvotedCount { get; set; }

        [Number(Name = "vd")]
        public int DownvotedCount { get; set; }

        public DbUserScores Apply(UserScores scores)
        {
            if (scores == null)
                return null;

            UploadCount         = scores.UploadCount;
            UploadApprovedCount = scores.UploadApprovedCount;
            EditCount           = scores.EditCount;
            EditApprovedCount   = scores.EditApprovedCount;
            UpvotedCount        = scores.UpvotedCount;
            DownvotedCount      = scores.DownvotedCount;

            return this;
        }

        public UserScores ApplyTo(UserScores scores)
        {
            scores.UploadCount         = UploadCount;
            scores.UploadApprovedCount = UploadApprovedCount;
            scores.EditCount           = EditCount;
            scores.EditApprovedCount   = EditApprovedCount;
            scores.UpvotedCount        = UpvotedCount;
            scores.DownvotedCount      = DownvotedCount;

            return scores;
        }
    }
}