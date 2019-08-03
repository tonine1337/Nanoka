using System.Runtime.Serialization;

namespace Nanoka.Core.Models
{
    /// <summary>
    /// Use bitwise complement to order by descending.
    /// </summary>
    public enum BooruQuerySort
    {
        [EnumMember(Value = "upload")] UploadTime,
        [EnumMember(Value = "update")] UpdateTime,
        [EnumMember(Value = "artist")] Artist,
        [EnumMember(Value = "character")] Character,
        [EnumMember(Value = "copyright")] Copyright,
        [EnumMember(Value = "metadata")] Metadata,
        [EnumMember(Value = "general")] General,
        [EnumMember(Value = "rating")] Rating,
        [EnumMember(Value = "score")] Score,
        [EnumMember(Value = "source")] Source,
        [EnumMember(Value = "width")] Width,
        [EnumMember(Value = "height")] Height,
        [EnumMember(Value = "size")] SizeInBytes
    }
}