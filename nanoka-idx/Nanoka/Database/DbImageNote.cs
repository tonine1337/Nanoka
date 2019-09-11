using Nanoka.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Database
{
    // nested object of DbImage
    public class DbImageNote
    {
        [Number(NumberType.Integer, Name = "x", Index = false), JsonProperty("x")]
        public int X { get; set; }

        [Number(NumberType.Integer, Name = "y", Index = false), JsonProperty("y")]
        public int Y { get; set; }

        [Number(NumberType.Integer, Name = "w", Index = false), JsonProperty("w")]
        public int Width { get; set; }

        [Number(NumberType.Integer, Name = "h", Index = false), JsonProperty("h")]
        public int Height { get; set; }

        [Text(Name = "c", Index = false), JsonProperty("c")]
        public string Content { get; set; }

        public ImageNote ToNote() => new ImageNote
        {
            X       = X,
            Y       = Y,
            Width   = Width,
            Height  = Height,
            Content = Content
        };

        public static DbImageNote FromNote(ImageNote note) => new DbImageNote
        {
            X       = note.X,
            Y       = note.Y,
            Width   = note.Width,
            Height  = note.Height,
            Content = note.Content
        };
    }
}