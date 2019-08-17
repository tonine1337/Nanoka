using System.Runtime.Serialization;

namespace Nanoka.Models.Query
{
    public enum QueryStrictness
    {
        [EnumMember(Value = "must")] Must = 0,
        [EnumMember(Value = "should")] Should = 1
    }
}