using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum SnapshotEvent
    {
        [EnumMember(Value = "unspecified")] Unspecified = 0,

        /// <summary>
        /// The snapshot was created when an object was created.
        /// In this case, <see cref="Snapshot{T}.Value"/> is null.
        /// </summary>
        [EnumMember(Value = "create")] Creation = 1,

        /// <summary>
        /// The snapshot was created when an object was modified.
        /// </summary>
        [EnumMember(Value = "modify")] Modification = 2,

        /// <summary>
        /// The snapshot was created when an object was deleted.
        /// </summary>
        [EnumMember(Value = "delete")] Deletion = 3,

        /// <summary>
        /// The snapshot was created when an object was reverted to a previous snapshot.
        /// </summary>
        [EnumMember(Value = "revert")] Rollback = 4
    }
}