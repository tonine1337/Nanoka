using Newtonsoft.Json;

namespace Nanoka.Core.Models
{
    public class UserScores
    {
        [JsonProperty("upload")]
        public int UploadCount { get; set; }

        [JsonProperty("upload_approved")]
        public int UploadApprovedCount { get; set; }

        [JsonProperty("edit")]
        public int EditCount { get; set; }

        [JsonProperty("edit_approved")]
        public int EditApprovedCount { get; set; }

        [JsonProperty("upvoted")]
        public int UpvotedCount { get; set; }

        [JsonProperty("downvoted")]
        public int DownvotedCount { get; set; }
    }
}
