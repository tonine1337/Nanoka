using System.Runtime.Serialization;

namespace Nanoka.Core.Models
{
    /// <summary>
    /// Use bitwise complement to order by descending.
    /// </summary>
    public enum BooruQuerySort
    {
        [EnumMember(Value = "relevance")] Relevance = 0,
        [EnumMember(Value = "upload")] UploadTime = 1,
        [EnumMember(Value = "update")] UpdateTime = 2,
        [EnumMember(Value = "rating")] Rating = 3,
        [EnumMember(Value = "score")] Score = 4,
        [EnumMember(Value = "width")] Width = 5,
        [EnumMember(Value = "height")] Height = 6
    }
}