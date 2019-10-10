using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum BookTag
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
        /// Tag is a parody.
        /// </summary>
        /// <remarks>
        /// Not to be confused with <see cref="Series"/>.
        /// </remarks>
        [EnumMember(Value = "parody")] Parody = 2,

        /// <summary>
        /// Tag is a character.
        /// </summary>
        [EnumMember(Value = "character")] Character = 3,

        /// <summary>
        /// Tag is a convention.
        /// </summary>
        [EnumMember(Value = "convention")] Convention = 4,

        /// <summary>
        /// Tag is a series.
        /// </summary>
        /// <remarks>
        /// Not to be confused with <see cref="Parody"/>.
        /// </remarks>
        [EnumMember(Value = "series")] Series = 5
    }
}