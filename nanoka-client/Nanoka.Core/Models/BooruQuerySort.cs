using System.Runtime.Serialization;

namespace Nanoka.Core.Models
{
    /// <summary>
    /// Use bitwise complement to order by descending.
    /// </summary>
    public enum BooruQuerySort
    {
        [EnumMember(Value = "relevance")] Relevance = 0,
        [EnumMember(Value = "upload")] UploadTime,
        [EnumMember(Value = "update")] UpdateTime,
        [EnumMember(Value = "rating")] Rating,
        [EnumMember(Value = "score")] Score,
        [EnumMember(Value = "width")] Width,
        [EnumMember(Value = "height")] Height,
        [EnumMember(Value = "size")] SizeInBytes
    }
}
