namespace Nanoka.Models
{
    public enum SnapshotTarget
    {
        /// <summary>
        /// Catch-all target.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Snapshot of a <see cref="Models.Doujinshi"/> object.
        /// </summary>
        Doujinshi = 1,

        /// <summary>
        /// Snapshot of a <see cref="Models.BooruPost"/> object.
        /// </summary>
        BooruPost = 2
    }
}