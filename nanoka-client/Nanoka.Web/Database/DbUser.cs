using System;
using Nanoka.Core.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Web.Database
{
    [ElasticsearchType(RelationName = nameof(User), IdProperty = nameof(Id))]
    public class DbUser
    {
        [Keyword]
        public string Id { get; set; }

        [Keyword(Name = "sec", Index = false)]
        public string Secret { get; set; }

        [Text(Name = "un")]
        public string Username { get; set; }

        [Date(Name = "r")]
        public DateTime Registered { get; set; }

        [Boolean(Name = "res")]
        public bool IsRestricted { get; set; }

        [Number(Name = "perm")]
        public UserPermissions Permissions { get; set; }

        [JsonProperty("upload")]
        public int UploadCount { get; set; }

        [JsonProperty("edit")]
        public int EditCount { get; set; }

        [JsonProperty("upvoted")]
        public int UpvotedCount { get; set; }

        [JsonProperty("downvoted")]
        public int DownvotedCount { get; set; }

        public DbUser Apply(User user)
        {
            if (user == null)
                return null;

            Id             = user.Id.ToShortString();
            Secret         = user.Secret.ToShortString();
            Username       = user.Username ?? Username;
            Registered     = user.Registered;
            IsRestricted   = user.IsRestricted;
            Permissions    = user.Permissions;
            UploadCount    = user.UploadCount;
            EditCount      = user.EditCount;
            UpvotedCount   = user.UpvotedCount;
            DownvotedCount = user.DownvotedCount;

            return this;
        }

        public User ApplyTo(User user)
        {
            user.Id             = Id.ToGuid();
            user.Secret         = Secret.ToGuid();
            user.Username       = Username ?? user.Username;
            user.Registered     = Registered;
            user.IsRestricted   = IsRestricted;
            user.Permissions    = Permissions;
            user.UploadCount    = UploadCount;
            user.EditCount      = EditCount;
            user.UpvotedCount   = UpvotedCount;
            user.DownvotedCount = DownvotedCount;
            user.Reputation     = UserReputationCalculator.Calc(user);

            return user;
        }
    }
}