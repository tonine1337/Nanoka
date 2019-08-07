namespace Nanoka.Core.Models
{
    public enum SnapshotTarget
    {
        /// <summary>
        /// Catch-all target.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Snapshot of a <see cref="Nanoka.Core.Models.Doujinshi"/> object.
        /// </summary>
        Doujinshi = 1,

        /// <summary>
        /// Snapshot of a <see cref="Nanoka.Core.Models.BooruPost"/> object.
        /// </summary>
        BooruPost = 2
    }
}