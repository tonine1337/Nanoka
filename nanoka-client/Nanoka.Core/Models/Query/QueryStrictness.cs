using System.Runtime.Serialization;

namespace Nanoka.Core.Models.Query
{
    public enum QueryStrictness
    {
        [EnumMember(Value = "must")] Must = 0,
        [EnumMember(Value = "should")] Should,
        [EnumMember(Value = "filter")] Filter
    }
}