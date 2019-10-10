using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum ImageTag
    {
        /// <summary>
        /// Tag has no specific type.
        /// </summary>
        [EnumMember(Value = "tag")] General = 0,

        /// <summary>
        /// Tag is an artist.
        /// </summary>
        [EnumMember(Value = "artist")] Artist = 1,

        /// <summary>
        /// Tag is a character.
        /// </summary>
        [EnumMember(Value = "character")] Character = 2,

        /// <summary>
        /// Tag is a copyright.
        /// </summary>
        [EnumMember(Value = "copyright")] Copyright = 3,

        /// <summary>
        /// Tag is metadata.
        /// </summary>
        [EnumMember(Value = "metadata")] Metadata = 4,

        /// <summary>
        /// Tag references a pool name.
        /// </summary>
        [EnumMember(Value = "pool")] Pool = 5
    }
}