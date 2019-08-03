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
    }
}