using System;
using Nanoka.Core;
using Nanoka.Core.Models;
using Nest;

namespace Nanoka.Web.Database
{
    [ElasticsearchType(RelationName = nameof(BooruPost), IdProperty = nameof(Id))]
    public class DbBooruPost
    {
        public string Id { get; set; }

        [Date(Name = "up")]
        public DateTime UploadTime { get; set; }

        [Date(Name = "ud")]
        public DateTime UpdateTime { get; set; }

        [Text(Name = "a")]
        public string[] Artist { get; set; }

        [Text(Name = "c")]
        public string[] Character { get; set; }

        [Text(Name = "cp")]
        public string[] Copyright { get; set; }

        [Text(Name = "m")]
        public string[] Metadata { get; set; }

        [Text(Name = "g")]
        public string[] General { get; set; }

        [Number(Name = "ra")]
        public BooruRating Rating { get; set; }

        [Number(Name = "sc")]
        public int Score { get; set; }

        [Text(Name = "src")]
        public string Source { get; set; }

        [Number(Name = "w")]
        public int Width { get; set; }

        [Number(Name = "h")]
        public int Height { get; set; }

        [Number(Name = "s")]
        public int SizeInBytes { get; set; }

        [Text(Name = "t")]
        public string MediaType { get; set; }

        [Text(Name = "sib")]
        public string[] SiblingIds { get; set; }

        public void Apply(BooruPost post)
        {
            Id          = post.Id.ToShortString();
            UploadTime  = post.UploadTime;
            UpdateTime  = post.UpdateTime;
            Artist      = post.Tags?.GetOrDefault(BooruTag.Artist) ?? Artist;
            Character   = post.Tags?.GetOrDefault(BooruTag.Character) ?? Character;
            Copyright   = post.Tags?.GetOrDefault(BooruTag.Copyright) ?? Copyright;
            Metadata    = post.Tags?.GetOrDefault(BooruTag.Metadata) ?? Metadata;
            General     = post.Tags?.GetOrDefault(BooruTag.General) ?? General;
            Rating      = post.Rating;
            Score       = post.Score;
            Source      = post.Source ?? Source;
            Width       = post.Width;
            Height      = post.Height;
            SizeInBytes = post.SizeInBytes;
            MediaType   = post.MediaType ?? MediaType;
            SiblingIds  = post.SiblingIds?.ToArray(x => x.ToShortString()) ?? SiblingIds;
        }

        public void ApplyTo(BooruPost post)
        {
            post.Id         = Id.ToGuid();
            post.UploadTime = UploadTime;
            post.UpdateTime = UpdateTime;

            post.Tags = Extensions.BuildArrayDict(
                            (BooruTag.Artist, Artist),
                            (BooruTag.Character, Character),
                            (BooruTag.Copyright, Copyright),
                            (BooruTag.Metadata, Metadata),
                            (BooruTag.General, General)) ?? post.Tags;

            post.Rating      = Rating;
            post.Score       = Score;
            post.Source      = Source ?? post.Source;
            post.Width       = Width;
            post.Height      = Height;
            post.SizeInBytes = SizeInBytes;
            post.MediaType   = MediaType ?? post.MediaType;
            post.SiblingIds  = SiblingIds?.ToArray(x => x.ToGuid()) ?? post.SiblingIds;
        }
    }
}