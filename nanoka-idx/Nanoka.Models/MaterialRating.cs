using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum MaterialRating
    {
        [EnumMember(Value = "safe")] Safe = 0,
        [EnumMember(Value = "questionable")] Questionable = 1,
        [EnumMember(Value = "explicit")] Explicit = 2
    }
}