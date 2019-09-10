using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum ImageMediaType
    {
        [EnumMember(Value = "unknown")] Unknown = 0,

        /// <summary>
        /// image/jpeg
        /// </summary>
        [EnumMember(Value = "jpeg")] Jpeg,

        /// <summary>
        /// image/png
        /// </summary>
        [EnumMember(Value = "png")] Png,

        /// <summary>
        /// image/gif
        /// </summary>
        [EnumMember(Value = "gif")] Gif
    }
}