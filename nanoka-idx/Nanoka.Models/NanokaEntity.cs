using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum NanokaEntity
    {
        /// <summary>
        /// <see cref="User"/>
        /// </summary>
        [EnumMember(Value = "user")] User = 0,

        /// <summary>
        /// <see cref="Book"/>
        /// </summary>
        [EnumMember(Value = "book")] Book = 1,

        /// <summary>
        /// <see cref="Image"/>
        /// </summary>
        [EnumMember(Value = "image")] Image = 2,

        /// <summary>
        /// <see cref="Song"/>
        /// </summary>
        [EnumMember(Value = "song")] Song = 3,

        /// <summary>
        /// <see cref="Snapshot"/>
        /// </summary>
        [EnumMember(Value = "snapshot")] Snapshot = 4,

        /// <summary>
        /// <see cref="Vote"/>
        /// </summary>
        [EnumMember(Value = "vote")] Vote = 5
    }
}