using System;
using System.Collections.Generic;
using Nanoka.Core;
using Nanoka.Core.Models;
using Nest;

namespace Nanoka.Web.Database
{
    [ElasticsearchType(RelationName = nameof(User), IdProperty = nameof(Id))]
    public class DbUser
    {
        [Keyword(Name = "id", Index = false)]
        public string Id { get; set; }

        [Keyword(Name = "sec", Index = false)]
        public string Secret { get; set; }

        [Keyword(Name = "un")]
        public string Username { get; set; }

        [Date(Name = "r")]
        public DateTime Registered { get; set; }

        [Nested(Name = "res")]
        public List<DbUserRestriction> Restrictions { get; set; }

        [Number(Name = "perm")]
        public UserPermissions Permissions { get; set; }

        [Number(Name = "c_up")]
        public int UploadCount { get; set; }

        [Number(Name = "c_ed")]
        public int EditCount { get; set; }

        [Number(Name = "c_uv")]
        public int UpvotedCount { get; set; }

        [Number(Name = "c_dv")]
        public int DownvotedCount { get; set; }

        public DbUser Apply(User user)
        {
            if (user == null)
                return null;

            Id         = user.Id.ToShortString();
            Secret     = user.Secret.ToShortString();
            Username   = user.Username ?? Username;
            Registered = user.Registered;

            Restrictions = user.Restrictions?.ToList(r => new DbUserRestriction().Apply(r)) ?? Restrictions;

            Permissions    = user.Permissions;
            UploadCount    = user.UploadCount;
            EditCount      = user.EditCount;
            UpvotedCount   = user.UpvotedCount;
            DownvotedCount = user.DownvotedCount;

            return this;
        }

        public User ApplyTo(User user)
        {
            user.Id         = Id.ToGuid();
            user.Secret     = Secret.ToGuid();
            user.Username   = Username ?? user.Username;
            user.Registered = Registered;

            user.Restrictions = Restrictions?.ToList(r => r.ApplyTo(new UserRestriction())) ?? user.Restrictions;

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