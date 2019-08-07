using System;
using System.Threading;
using Newtonsoft.Json;

namespace Nanoka.Web
{
    public class UploadWorker : IDisposable
    {
        readonly object _lock = new object();
        readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        double _progress;
        DateTime? _end;

        [JsonProperty("id")]
        public Guid Id { get; } = Guid.NewGuid();

        [JsonProperty("progress")]
        public double Progress
        {
            get
            {
                lock (_lock)
                    return _progress;
            }
        }

        [JsonProperty("start")]
        public DateTime Start { get; } = DateTime.UtcNow;

        [JsonProperty("end")]
        public DateTime End
        {
            get
            {
                lock (_lock)
                {
                    if (_end != null)
                        return _end.Value;

                    if (_progress <= 0 || _progress > 1)
                        return DateTime.MaxValue;

                    // this is a bad estimate; todo: use moving average
                    var now = DateTime.UtcNow;

                    return now + (now - Start) / _progress;
                }
            }
        }

        [JsonProperty("running")]
        public bool IsRunning { get; private set; } = true;

        [JsonProperty("message")]
        public string Message { get; private set; }

        [JsonIgnore]
        public CancellationToken CancellationToken => _cancellationToken.Token;

        public void SetProgress(double value, string message = null)
        {
            lock (_lock)
            {
                value = Math.Clamp(value, 0, 1);

                IsRunning = false; // value < 1; 1 does not necessarily indicate completion
                Message   = message ?? Message;

                _end      = null;
                _progress = value;
            }
        }

        public void SetFailure(string message)
        {
            lock (_lock)
            {
                IsRunning = false;
                Message   = Message ?? message;

                _end = DateTime.UtcNow;
            }
        }

        public void SetSuccess(string message = null)
        {
            lock (_lock)
            {
                IsRunning = true;
                Message   = Message ?? message;

                _end      = DateTime.UtcNow;
                _progress = 1;
            }
        }

        public void Cancel() => _cancellationToken.Cancel();

        public void Dispose()
        {
            _cancellationToken.Cancel();
            _cancellationToken.Dispose();
        }
    }
}