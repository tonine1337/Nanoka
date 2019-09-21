namespace Nanoka.Storage
{
    public class SoftDeleteOptions
    {
        /// <summary>
        /// Time in milliseconds to wait before hard deleting a soft deleted file.
        /// </summary>
        public double WaitMs { get; set; } = 1000 * 60 * 60 * 24 * 14; // 14 days

        /// <summary>
        /// Time in minutes between checking for any soft deleted files due for hard deletion.
        /// </summary>
        public double CheckIntervalMin { get; set; } = 10; // 10 minutes
    }
}