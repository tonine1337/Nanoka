using System.Runtime.Serialization;

namespace Nanoka.Models
{
    /// <summary>
    /// Use bitwise complement to order by descending.
    /// </summary>
    public enum DoujinshiQuerySort
    {
        [EnumMember(Value = "relevance")] Relevance = 0,
        [EnumMember(Value = "upload")] UploadTime = 1,
        [EnumMember(Value = "update")] UpdateTime = 2,
        [EnumMember(Value = "name_original")] OriginalName = 3,
        [EnumMember(Value = "name_romanized")] RomanizedName = 4,
        [EnumMember(Value = "name_english")] EnglishName = 5,
        [EnumMember(Value = "score")] Score = 6,
        [EnumMember(Value = "pages")] PageCount = 7
    }
}