using System.Runtime.Serialization;

namespace Nanoka.Core.Models
{
    public enum BooruTag
    {
        [EnumMember(Value = "artist")] Artist = 0,
        [EnumMember(Value = "character")] Character = 1,
        [EnumMember(Value = "copyright")] Copyright = 2,
        [EnumMember(Value = "metadata")] Metadata = 3,
        [EnumMember(Value = "general")] General = 4
    }
}