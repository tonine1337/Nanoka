using System;
using System.Diagnostics;

namespace Nanoka.Core
{
    public sealed class MeasureContext : IDisposable
    {
        readonly Stopwatch _watch = Stopwatch.StartNew();

        public TimeSpan Elapsed
        {
            get
            {
                _watch.Stop();
                return _watch.Elapsed;
            }
        }

        public double Minutes => Elapsed.TotalMinutes;
        public double Seconds => Elapsed.TotalSeconds;
        public double Milliseconds => Elapsed.TotalMilliseconds;

        public override string ToString() => Elapsed.ToString();

        public void Dispose() => _watch.Stop();
    }
}
