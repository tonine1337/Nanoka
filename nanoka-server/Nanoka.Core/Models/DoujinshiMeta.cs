using System.Runtime.Serialization;

namespace Nanoka.Core.Models
{
    public enum DoujinshiMeta
    {
        [EnumMember(Value = "artist")] Artist = 0,
        [EnumMember(Value = "group")] Group = 1,
        [EnumMember(Value = "parody")] Parody = 2,
        [EnumMember(Value = "character")] Character = 3,
        [EnumMember(Value = "language")] Language = 4,
        [EnumMember(Value = "tag")] Tag = 5,
        [EnumMember(Value = "convention")] Convention = 6
    }
}