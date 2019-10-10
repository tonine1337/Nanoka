using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum QueryMatchMode
    {
        /// <summary>
        /// If the query mode is "any" and contains multiple values, at least one of them must be matched.
        /// </summary>
        [EnumMember(Value = "any")] Any = 0,

        /// <summary>
        /// If the query mode is "all" and contains multiple values, all of them must be matched.
        /// </summary>
        [EnumMember(Value = "all")] All = 1
    }
}