using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum SnapshotType
    {
        /// <summary>
        /// Snapshot was created by a process of the system.
        /// </summary>
        [EnumMember(Value = "system")] System = 0,

        /// <summary>
        /// Snapshot was created by a normal user.
        /// </summary>
        [EnumMember(Value = "user")] User = 1,

        /// <summary>
        /// Snapshot was created by a moderator user.
        /// </summary>
        [EnumMember(Value = "moderator")] Moderator = 2
    }
}