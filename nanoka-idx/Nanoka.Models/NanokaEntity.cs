using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum NanokaEntity
    {
        [EnumMember(Value = "unknown")] Unknown = 0,
        [EnumMember(Value = "user")] User = 1,
        [EnumMember(Value = "book")] Book = 2,
        [EnumMember(Value = "image")] Image = 3,
        [EnumMember(Value = "song")] Song = 4,
        [EnumMember(Value = "snapshot")] Snapshot = 5,
        [EnumMember(Value = "vote")] Vote = 6
    }
}