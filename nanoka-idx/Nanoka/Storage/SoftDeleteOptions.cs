namespace Nanoka.Storage
{
    public class SoftDeleteOptions
    {
        /// <summary>
        /// Time in milliseconds to wait before hard deleting a soft deleted file.
        /// </summary>
        public double WaitMs { get; set; } = 1000 * 60 * 60 * 24 * 14; // 14 days
    }
}