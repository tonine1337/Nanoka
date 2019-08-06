namespace Nanoka.Core.Models
{
    public enum SnapshotEvent
    {
        /// <summary>
        /// Catch-all event.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Doujinshi object was modified.
        /// </summary>
        DoujinshiModified = 1,

        /// <summary>
        /// Doujinshi object was deleted.
        /// </summary>
        DoujinshiDeleted = 2
    }
}