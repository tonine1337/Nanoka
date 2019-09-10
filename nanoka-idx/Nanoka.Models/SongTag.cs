using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum SongTag
    {
        [EnumMember(Value = "tag")] General = 0,
        [EnumMember(Value = "artist")] Artist = 1,
        [EnumMember(Value = "album")] Album = 2,
        [EnumMember(Value = "year")] Year = 3,
        [EnumMember(Value = "genre")] Genre = 4,
        [EnumMember(Value = "source")] Source = 5,
        [EnumMember(Value = "convention")] Convention = 6
    }
}