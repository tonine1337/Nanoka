using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum VoteType
    {
        /// <summary>
        /// Vote is positive.
        /// </summary>
        [EnumMember(Value = "up")] Up = 0,

        /// <summary>
        /// Vote is negative.
        /// </summary>
        [EnumMember(Value = "down")] Down = 1
    }
}