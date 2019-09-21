using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum QueryMatchMode
    {
        [EnumMember(Value = "any")] Any = 0,
        [EnumMember(Value = "all")] All
    }
}