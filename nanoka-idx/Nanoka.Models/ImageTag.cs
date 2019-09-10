using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum ImageTag
    {
        [EnumMember(Value = "general")] General = 0,
        [EnumMember(Value = "artist")] Artist = 1,
        [EnumMember(Value = "character")] Character = 2,
        [EnumMember(Value = "copyright")] Copyright = 3,
        [EnumMember(Value = "metadata")] Metadata = 4,
        [EnumMember(Value = "pool")] Pool = 5,
        [EnumMember(Value = "source")] Source = 6
    }
}