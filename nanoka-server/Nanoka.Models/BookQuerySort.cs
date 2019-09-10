using System.Runtime.Serialization;

namespace Nanoka.Models
{
    /// <summary>
    /// Use bitwise complement to order by descending.
    /// </summary>
    public enum BookQuerySort
    {
        [EnumMember(Value = "relevance")] Relevance = 0,
        [EnumMember(Value = "upload")] UploadTime = 1,
        [EnumMember(Value = "update")] UpdateTime = 2,
        [EnumMember(Value = "score")] Score = 3,
        [EnumMember(Value = "pages")] PageCount = 4
    }
}