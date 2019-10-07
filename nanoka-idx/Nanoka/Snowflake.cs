using System;
using System.Diagnostics;
using System.Threading;

namespace Nanoka
{
    public static class Snowflake
    {
        // stopwatch is used for higher accuracy timestamps
        static readonly TimeSpan _watchOffset = DateTime.UtcNow.Subtract(new DateTime(2000, 1, 1));
        static readonly Stopwatch _watch = Stopwatch.StartNew();

        static long _lastTimestamp;

        /// <summary>
        /// Timestamp with millisecond accuracy from year 2000 which avoids conflicts.
        /// </summary>
        static long Timestamp
        {
            get
            {
                long original,
                     newValue;

                do
                {
                    original = _lastTimestamp;

                    var now = (long) (_watchOffset + _watch.Elapsed).TotalMilliseconds;

                    newValue = Math.Max(now, original + 1);
                }
                while (Interlocked.CompareExchange(ref _lastTimestamp, newValue, original) != original);

                return newValue;
            }
        }

        /// <summary>
        /// Generates a short time-based
        /// </summary>
        public static string New
        {
            get
            {
                var buffer = BitConverter.GetBytes(Timestamp);
                Array.Reverse(buffer);

                var offset = 0;

                while (buffer[offset] == 0)
                    offset++;

                return Convert.ToBase64String(buffer, offset, buffer.Length - offset)
                              .TrimEnd('=')
                              .Replace("/", "_")
                              .Replace("+", "-");
            }
        }
    }
}