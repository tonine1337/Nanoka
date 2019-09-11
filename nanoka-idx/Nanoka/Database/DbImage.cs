using System.Collections.Generic;
using Nanoka.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Database
{
    [ElasticsearchType(IdProperty = nameof(Id), RelationName = nameof(Image))]
    public class DbImage
    {
        [Keyword(Name = "id", Index = false), JsonProperty("id")]
        public int Id { get; set; }

        [Number(NumberType.Double, Name = "s"), JsonProperty("s")]
        public double Score { get; set; }

        [Number(NumberType.Integer, Name = "sw"), JsonProperty("w")]
        public int Width { get; set; }

        [Number(NumberType.Integer, Name = "sh"), JsonProperty("h")]
        public int Height { get; set; }

        [Keyword(Name = "t"), JsonProperty("t")]
        public ImageMediaType MediaType { get; set; }

        [Text(Name = "tg"), JsonProperty("tg")]
        public string[] TagsGeneral { get; set; }

        [Text(Name = "ta"), JsonProperty("ta")]
        public string[] TagsArtist { get; set; }

        [Text(Name = "tc"), JsonProperty("tc")]
        public string[] TagsCharacter { get; set; }

        [Text(Name = "tcp"), JsonProperty("tcp")]
        public string[] TagsCopyright { get; set; }

        [Text(Name = "tm"), JsonProperty("tm")]
        public string[] TagsMetadata { get; set; }

        [Text(Name = "tp"), JsonProperty("tp")]
        public string[] TagsPool { get; set; }

        [Keyword(Name = "src"), JsonProperty("src")]
        public string[] Sources { get; set; }

        [Keyword(Name = "r"), JsonProperty("r")]
        public MaterialRating Rating { get; set; }

        [Nested(Name = "note"), JsonProperty("note")]
        public DbImageNote[] Notes { get; set; }

#region Cached

        [Number(NumberType.Integer, Name = "note_c"), JsonIgnore]
        public int NoteCount { get; set; }

#endregion

        public Image ToImage() => new Image
        {
            Id        = Id,
            Score     = Score,
            Width     = Width,
            Height    = Height,
            MediaType = MediaType,
            Tags = new Dictionary<ImageTag, string[]>
            {
                { ImageTag.General, TagsGeneral },
                { ImageTag.Artist, TagsArtist },
                { ImageTag.Character, TagsCharacter },
                { ImageTag.Copyright, TagsCopyright },
                { ImageTag.Metadata, TagsMetadata },
                { ImageTag.Pool, TagsPool }
            }.RemoveNullValues(),
            Sources = Sources.ToArray(ExternalSource.Parse),
            Rating  = Rating,
            Notes   = Notes.ToArray(n => n.ToNote())
        };

        public static DbImage FromImage(Image image)
        {
            var notes = image.Notes.ToArray(DbImageNote.FromNote);

            return new DbImage
            {
                Id            = image.Id,
                Score         = image.Score,
                Width         = image.Width,
                Height        = image.Height,
                MediaType     = image.MediaType,
                TagsGeneral   = image.Tags.GetValueOrDefault(ImageTag.General),
                TagsArtist    = image.Tags.GetValueOrDefault(ImageTag.Artist),
                TagsCharacter = image.Tags.GetValueOrDefault(ImageTag.Character),
                TagsCopyright = image.Tags.GetValueOrDefault(ImageTag.Copyright),
                TagsMetadata  = image.Tags.GetValueOrDefault(ImageTag.Metadata),
                TagsPool      = image.Tags.GetValueOrDefault(ImageTag.Pool),
                Sources       = image.Sources.ToArray(s => s.ToString()),
                Rating        = image.Rating,
                Notes         = notes,
                NoteCount     = notes.Length
            };
        }
    }
}