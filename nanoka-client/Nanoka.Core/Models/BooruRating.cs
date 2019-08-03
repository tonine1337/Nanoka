using System.Runtime.Serialization;

namespace Nanoka.Core.Models
{
    public enum BooruRating
    {
        [EnumMember(Value = "safe")] Safe,
        [EnumMember(Value = "questionable")] Questionable,
        [EnumMember(Value = "explicit")] Explicit
    }
}