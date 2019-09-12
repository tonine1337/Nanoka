using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum NanokaEntity
    {
        [EnumMember(Value = "user")] User = 0,
        [EnumMember(Value = "book")] Book = 1,
        [EnumMember(Value = "image")] Image = 2,
        [EnumMember(Value = "song")] Song = 3,
        [EnumMember(Value = "snapshot")] Snapshot = 4,
        [EnumMember(Value = "vote")] Vote = 5
    }
}