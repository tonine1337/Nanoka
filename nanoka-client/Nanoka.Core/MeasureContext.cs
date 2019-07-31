using System;
using System.Diagnostics;

namespace Nanoka.Core
{
    public sealed class MeasureContext : IDisposable
    {
        readonly Stopwatch _watch = Stopwatch.StartNew();

        public MeasureContext(out Func<double> time)
        {
            time = () =>
            {
                _watch.Stop();
                return _watch.Elapsed.TotalSeconds;
            };
        }

        public void Dispose() => _watch.Stop();
    }
}
