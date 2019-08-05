using System.Runtime.Serialization;

namespace Nanoka.Core.Models.Query
{
    public enum QueryStrictness
    {
        [EnumMember(Value = "must")] Must,
        [EnumMember(Value = "should")] Should
    }
}