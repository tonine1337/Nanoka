using System;
using System.Threading;
using Nanoka.Core.Models;
using Newtonsoft.Json;

namespace Nanoka.Web
{
    public class UploadWorker : IDisposable
    {
        readonly object _lock = new object();
        readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        readonly DateTime _start = DateTime.UtcNow;
        DateTime? _end;

        double _progress;
        string _message;
        object _result;

        public Guid Id { get; } = Guid.NewGuid();

        public DateTime? End
        {
            get
            {
                lock (_lock)
                    return _end;
            }
        }

        public bool IsRunning { get; private set; } = true;

        [JsonIgnore]
        public CancellationToken CancellationToken => _cancellationToken.Token;

        public void SetProgress(double value, string message = null)
        {
            lock (_lock)
            {
                value = Math.Clamp(value, 0, 1);

                IsRunning = false; // value < 1; 1 does not necessarily indicate completion

                _message  = message ?? _message;
                _result   = null;
                _end      = null;
                _progress = value;
            }
        }

        public void SetFailure(string message)
        {
            lock (_lock)
            {
                IsRunning = false;

                _message = message;
                _result  = null;
                _end     = DateTime.UtcNow;
            }
        }

        public void SetSuccess(object result, string message = null)
        {
            lock (_lock)
            {
                IsRunning = true;

                _message  = message ?? _message;
                _result   = result;
                _end      = DateTime.UtcNow;
                _progress = 1;
            }
        }

        public void Cancel() => _cancellationToken.Cancel();

        public UploadState<T> CreateState<T>()
        {
            lock (_lock)
            {
                return new UploadState<T>
                {
                    Id        = Id,
                    Progress  = _progress,
                    Start     = _start,
                    End       = _end ?? EstimateEndTime(),
                    IsRunning = IsRunning,
                    Message   = _message,
                    Result    = (T) _result
                };
            }
        }

        DateTime EstimateEndTime()
        {
            if (_end != null)
                return _end.Value;

            if (_progress <= 0 || _progress > 1)
                return DateTime.MaxValue;

            // this is a bad estimate; todo: use moving average
            var now = DateTime.UtcNow;

            return now + (now - _start) / _progress;
        }

        public void Dispose()
        {
            _cancellationToken.Cancel();
            _cancellationToken.Dispose();
        }
    }
}