using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum BookSort
    {
        /// <summary>
        /// Sort by relevance.
        /// </summary>
        [EnumMember(Value = "relevance")] Relevance = 0,

        /// <summary>
        /// Sort by <see cref="Book.Score"/>.
        /// </summary>
        [EnumMember(Value = "score")] Score = 1,

        /// <summary>
        /// Sort by the largest or smallest value (depending on direction) of <see cref="BookContent.PageCount"/>.
        /// </summary>
        [EnumMember(Value = "pageCount")] PageCount = 2
    }
}