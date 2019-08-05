using System.Runtime.Serialization;

namespace Nanoka.Core.Models
{
    /// <summary>
    /// Use bitwise complement to order by descending.
    /// </summary>
    public enum DoujinshiQuerySort
    {
        [EnumMember(Value = "relevance")] Relevance = 0,
        [EnumMember(Value = "upload")] UploadTime,
        [EnumMember(Value = "update")] UpdateTime,
        [EnumMember(Value = "name_original")] OriginalName,
        [EnumMember(Value = "name_romanized")] RomanizedName,
        [EnumMember(Value = "name_english")] EnglishName,
        [EnumMember(Value = "score")] Score,
        [EnumMember(Value = "pages")] PageCount
    }
}