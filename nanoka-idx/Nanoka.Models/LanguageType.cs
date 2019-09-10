using System.Runtime.Serialization;

namespace Nanoka.Models
{
    /// <summary>
    /// A minimal list of common languages.
    /// </summary>
    public enum LanguageType
    {
        [EnumMember(Value = "jp")] Japanese = 0,
        [EnumMember(Value = "en")] English = 1,
        [EnumMember(Value = "jp_en")] JapaneseRomanized = 2, // not a language
        [EnumMember(Value = "zh_hans")] ChineseSimplified = 3,
        [EnumMember(Value = "zh_hant")] ChineseTraditional = 4,
        [EnumMember(Value = "it")] Italian = 5,
        [EnumMember(Value = "es")] Spanish = 6,
        [EnumMember(Value = "hi")] Hindi = 7,
        [EnumMember(Value = "de")] German = 8,
        [EnumMember(Value = "fr")] French = 9,
        [EnumMember(Value = "tr")] Turkish = 10,
        [EnumMember(Value = "pl")] Polish = 11,
        [EnumMember(Value = "nl")] Dutch = 12,
        [EnumMember(Value = "ru")] Russian = 13,
        [EnumMember(Value = "ko")] Korean = 14,
        [EnumMember(Value = "in")] Indonesian = 15,
        [EnumMember(Value = "vi")] Vietnamese = 16
    }
}