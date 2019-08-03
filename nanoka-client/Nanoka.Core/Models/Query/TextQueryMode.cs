using System.Runtime.Serialization;

namespace Nanoka.Core.Models.Query
{
    public enum TextQueryMode
    {
        [EnumMember(Value = "simple")] Simple = 0,
        [EnumMember(Value = "match")] Match,
        [EnumMember(Value = "phrase")] Phrase
    }
}