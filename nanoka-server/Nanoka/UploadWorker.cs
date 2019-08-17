using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nanoka.Models;

namespace Nanoka
{
    public class UploadWorker : IDisposable
    {
        readonly object _lock = new object();
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        readonly List<TaskCompletionSource<UploadState>> _changeListeners = new List<TaskCompletionSource<UploadState>>();

        readonly Guid _id;
        readonly IServiceProvider _services;

        readonly DateTime _start = DateTime.UtcNow;
        DateTime? _end;

        double _progress;
        string _message;

        public UploadWorker(Guid id, IServiceProvider services)
        {
            _id       = id;
            _services = services;
        }

        public DateTime? End
        {
            get
            {
                lock (_lock)
                    return _end;
            }
        }

        bool _isRunning = true;
        bool _isFailed = false;

        public void SetMessage(string message)
        {
            lock (_lock)
                _message = message;

            SignalChange();
        }

        public void SetProgress(double value, string message = null)
        {
            lock (_lock)
            {
                value = Math.Clamp(value, 0, 1);

                _isRunning = true;
                _isFailed  = false;

                _message  = message ?? _message;
                _end      = null;
                _progress = value;
            }

            SignalChange();
        }

        public void SetFailure(string message)
        {
            lock (_lock)
            {
                _isRunning = false;
                _isFailed  = true;

                _message = message;
                _end     = DateTime.UtcNow;
            }

            SignalChange();
        }

        public void SetSuccess(string message = null)
        {
            lock (_lock)
            {
                _isRunning = false;
                _isFailed  = false;

                _message  = message ?? _message;
                _end      = DateTime.UtcNow;
                _progress = 1;
            }

            SignalChange();
        }

        void SignalChange()
        {
            lock (_lock)
            {
                var state = CreateState();

                foreach (var source in _changeListeners)
                    source.TrySetResult(state);

                _changeListeners.Clear();
            }
        }

        public async Task<UploadState> WaitForStateChangeAsync(CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<UploadState> source;

            lock (_lock)
            {
                if (!_isRunning)
                    return CreateState();

                source = new TaskCompletionSource<UploadState>();

                _changeListeners.Add(source);
            }

            using (cancellationToken.Register(() => source.TrySetCanceled(cancellationToken), useSynchronizationContext: false))
                return await source.Task;
        }

        public void Cancel() => _cancellationTokenSource.Cancel();

        public delegate Task UploadWorkerDelegate(IServiceProvider services,
                                                  CancellationToken cancellationToken = default);

        public UploadState Start(UploadWorkerDelegate func)
        {
            Task.Run(
                async () =>
                {
                    try
                    {
                        using (var scope = _services.CreateScope())
                            await func(scope.ServiceProvider, _cancellationTokenSource.Token);

                        // worker function should have set the progress to 1 when it finished
                        if (_isRunning)
                            throw new InvalidOperationException("Worker ended prematurely due to an unknown reason.");
                    }
                    catch (TaskCanceledException)
                    {
                        SetFailure("Upload worker has been canceled.");
                    }
                    catch (Exception e)
                    {
                        _services.GetService<ILogger<UploadWorker>>().LogWarning(e, $"Upload worker '{_id}' died with exception");

                        SetFailure(e.Message);
                    }
                },
                _cancellationTokenSource.Token);

            return CreateState();
        }

        public UploadState CreateState()
        {
            lock (_lock)
            {
                return new UploadState
                {
                    Id        = _id,
                    Progress  = _progress,
                    Start     = _start,
                    End       = _end ?? EstimateEndTime(),
                    IsRunning = _isRunning,
                    IsFailed  = _isFailed,
                    Message   = _message
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
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}