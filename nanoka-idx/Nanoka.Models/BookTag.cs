using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum BookTag
    {
        [EnumMember(Value = "tag")] General = 0,
        [EnumMember(Value = "artist")] Artist = 1,
        [EnumMember(Value = "parody")] Parody = 2,
        [EnumMember(Value = "character")] Character = 3,
        [EnumMember(Value = "convention")] Convention = 4,
        [EnumMember(Value = "series")] Series = 5,
        [EnumMember(Value = "source")] Source = 6
    }
}