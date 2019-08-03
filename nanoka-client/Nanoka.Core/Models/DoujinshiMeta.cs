using System.Runtime.Serialization;

namespace Nanoka.Core.Models
{
    public enum DoujinshiMeta
    {
        [EnumMember(Value = "artist")] Artist,
        [EnumMember(Value = "group")] Group,
        [EnumMember(Value = "parody")] Parody,
        [EnumMember(Value = "character")] Character,
        [EnumMember(Value = "category")] Category,
        [EnumMember(Value = "language")] Language,
        [EnumMember(Value = "tag")] Tag,
        [EnumMember(Value = "convention")] Convention
    }
}