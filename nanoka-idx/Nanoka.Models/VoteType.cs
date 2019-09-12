using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum VoteType
    {
        [EnumMember(Value = "up")] Up = 0,
        [EnumMember(Value = "down")] Down = 1
    }
}