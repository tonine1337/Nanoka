using System.Runtime.Serialization;

namespace Nanoka.Core.Models
{
    /// <summary>
    /// Use bitwise complement to order by descending.
    /// </summary>
    public enum DoujinshiQuerySort
    {
        [EnumMember(Value = "upload")] UploadTime,
        [EnumMember(Value = "update")] UpdateTime,
        [EnumMember(Value = "name_original")] OriginalName,
        [EnumMember(Value = "name_romanized")] RomanizedName,
        [EnumMember(Value = "name_english")] EnglishName,
        [EnumMember(Value = "score")] Score,
        [EnumMember(Value = "artist")] Artist,
        [EnumMember(Value = "group")] Group,
        [EnumMember(Value = "parody")] Parody,
        [EnumMember(Value = "character")] Character,
        [EnumMember(Value = "category")] Category,
        [EnumMember(Value = "language")] Language,
        [EnumMember(Value = "tag")] Tag,
        [EnumMember(Value = "convention")] Convention,
        [EnumMember(Value = "source")] Source,
        [EnumMember(Value = "pages")] PageCount
    }
}