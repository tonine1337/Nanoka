using System;
using System.Collections.Generic;
using Nanoka.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Database
{
    [ElasticsearchType(RelationName = nameof(BooruPost), IdProperty = nameof(Id))]
    public class DbBooruPost
    {
        [Keyword(Name = "id", Index = false), JsonProperty("id")]
        public string Id { get; set; }

        [Keyword(Name = "upu"), JsonProperty("upu")]
        public string UploaderId { get; set; }

        [Date(Name = "up"), JsonProperty("up")]
        public DateTime UploadTime { get; set; }

        [Date(Name = "ud"), JsonProperty("ud")]
        public DateTime UpdateTime { get; set; }

        [Text(Name = "a"), JsonProperty("a")]
        public string[] Artist { get; set; }

        [Text(Name = "c"), JsonProperty("c")]
        public string[] Character { get; set; }

        [Text(Name = "cp"), JsonProperty("cp")]
        public string[] Copyright { get; set; }

        [Text(Name = "m"), JsonProperty("m")]
        public string[] Metadata { get; set; }

        [Text(Name = "g"), JsonProperty("g")]
        public string[] General { get; set; }

        [Number(Name = "ra"), JsonProperty("ga")]
        public BooruRating Rating { get; set; }

        [Number(Name = "sc"), JsonProperty("sc")]
        public int Score { get; set; }

        [Text(Name = "src"), JsonProperty("src")]
        public string Source { get; set; }

        [Number(Name = "w"), JsonProperty("w")]
        public int Width { get; set; }

        [Number(Name = "h"), JsonProperty("h")]
        public int Height { get; set; }

        [Keyword(Name = "t"), JsonProperty("t")]
        public string MediaType { get; set; }

        [Keyword(Name = "sib"), JsonProperty("sib")]
        public string[] SiblingIds { get; set; }

        public DbBooruPost Apply(BooruPost post)
        {
            if (post == null)
                return null;

            Id         = post.Id.ToShortString();
            UploaderId = post.UploaderId.ToShortString();
            UploadTime = post.UploadTime;
            UpdateTime = post.UpdateTime;
            Artist     = post.Tags?.GetOrDefault(BooruTag.Artist) ?? Artist;
            Character  = post.Tags?.GetOrDefault(BooruTag.Character) ?? Character;
            Copyright  = post.Tags?.GetOrDefault(BooruTag.Copyright) ?? Copyright;
            Metadata   = post.Tags?.GetOrDefault(BooruTag.Metadata) ?? Metadata;
            General    = post.Tags?.GetOrDefault(BooruTag.General) ?? General;
            Rating     = post.Rating;
            Score      = post.Score;
            Source     = post.Source ?? Source;
            Width      = post.Width;
            Height     = post.Height;
            MediaType  = post.MediaType ?? MediaType;
            SiblingIds = post.SiblingIds?.ToArray(x => x.ToShortString()) ?? SiblingIds;

            return this;
        }

        public BooruPost ApplyTo(BooruPost post)
        {
            post.Id         = Id.ToGuid();
            post.UploaderId = UploaderId.ToGuid();
            post.UploadTime = UploadTime;
            post.UpdateTime = UpdateTime;

            post.Tags = new Dictionary<BooruTag, string[]>
            {
                { BooruTag.Artist, Artist ?? post.Tags?.GetOrDefault(BooruTag.Artist) },
                { BooruTag.Character, Character ?? post.Tags?.GetOrDefault(BooruTag.Character) },
                { BooruTag.Copyright, Copyright ?? post.Tags?.GetOrDefault(BooruTag.Copyright) },
                { BooruTag.Metadata, Metadata ?? post.Tags?.GetOrDefault(BooruTag.Metadata) },
                { BooruTag.General, General ?? post.Tags?.GetOrDefault(BooruTag.General) }
            };

            post.Rating     = Rating;
            post.Score      = Score;
            post.Source     = Source ?? post.Source;
            post.Width      = Width;
            post.Height     = Height;
            post.MediaType  = MediaType ?? post.MediaType;
            post.SiblingIds = SiblingIds?.ToList(x => x.ToGuid()) ?? post.SiblingIds;

            return post;
        }
    }
}