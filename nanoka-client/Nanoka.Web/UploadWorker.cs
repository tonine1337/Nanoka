using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nanoka.Core.Models;

namespace Nanoka.Web
{
    public class UploadWorker : IDisposable
    {
        readonly object _lock = new object();
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

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

        public void SetMessage(string message)
        {
            lock (_lock)
                _message = message;
        }

        public void SetProgress(double value, string message = null)
        {
            lock (_lock)
            {
                value = Math.Clamp(value, 0, 1);

                _isRunning = false; // value < 1; 1 does not necessarily indicate completion

                _message  = message ?? _message;
                _end      = null;
                _progress = value;
            }
        }

        public void SetFailure(string message)
        {
            lock (_lock)
            {
                _isRunning = false;

                _message = message;
                _end     = DateTime.UtcNow;
            }
        }

        public void SetSuccess(string message = null)
        {
            lock (_lock)
            {
                _isRunning = true;

                _message  = message ?? _message;
                _end      = DateTime.UtcNow;
                _progress = 1;
            }
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
                            SetFailure("Upload worker ended prematurely due to an unknown reason.");
                    }
                    catch (TaskCanceledException)
                    {
                        SetFailure("Upload worker has been canceled.");
                    }
                    catch (Exception e)
                    {
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