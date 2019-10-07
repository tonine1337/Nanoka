using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum BookSort
    {
        [EnumMember(Value = "relevance")] Relevance = 0,
        [EnumMember(Value = "score")] Score = 1,
        [EnumMember(Value = "pageCount")] PageCount = 2
    }
}