using System.Runtime.Serialization;

namespace Nanoka.Models
{
    public enum SnapshotEvent
    {
        /// <summary>
        /// The snapshot was created when an entity was created.
        /// </summary>
        [EnumMember(Value = "create")] Creation = 0,

        /// <summary>
        /// The snapshot was created when an entity was modified.
        /// </summary>
        [EnumMember(Value = "modify")] Modification = 1,

        /// <summary>
        /// The snapshot was created when an entity was deleted.
        /// In this case, <see cref="Snapshot{T}.Value"/> is null.
        /// </summary>
        [EnumMember(Value = "delete")] Deletion = 2,

        /// <summary>
        /// The snapshot was created when an entity was reverted to a previous snapshot.
        /// </summary>
        [EnumMember(Value = "revert")] Rollback = 3
    }
}