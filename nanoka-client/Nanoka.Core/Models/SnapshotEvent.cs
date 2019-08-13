namespace Nanoka.Core.Models
{
    public enum SnapshotEvent
    {
        /// <summary>
        /// Catch-all event.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// The snapshot was taken before a modification to an object.
        /// </summary>
        Modification = 1,

        /// <summary>
        /// The snapshot was taken before the deletion of an object.
        /// </summary>
        Deletion = 2,

        /// <summary>
        /// The snapshot was taken before an object was reverted to its previous state.
        /// </summary>
        Rollback = 3
    }
}