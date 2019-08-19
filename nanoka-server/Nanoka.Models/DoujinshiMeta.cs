using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum DoujinshiMeta
    {
        [EnumMember(Value = "artist")] Artist = 0,
        [EnumMember(Value = "group")] Group = 1,
        [EnumMember(Value = "parody")] Parody = 2,
        [EnumMember(Value = "character")] Character = 3,
        [EnumMember(Value = "tag")] Tag = 4,
        [EnumMember(Value = "convention")] Convention = 5
    }
}