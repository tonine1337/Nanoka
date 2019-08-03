using System.Runtime.Serialization;

namespace Nanoka.Core.Models
{
    public enum BooruTag
    {
        [EnumMember(Value = "artist")] Artist,
        [EnumMember(Value = "character")] Character,
        [EnumMember(Value = "copyright")] Copyright,
        [EnumMember(Value = "metadata")] Metadata,
        [EnumMember(Value = "general")] General
    }
}