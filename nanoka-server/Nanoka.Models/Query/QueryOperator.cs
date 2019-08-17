using System.Runtime.Serialization;

namespace Nanoka.Models.Query
{
    public enum QueryOperator
    {
        [EnumMember(Value = "all")] All = 0,
        [EnumMember(Value = "any")] Any = 1,
        [EnumMember(Value = "none")] None = 2
    }
}