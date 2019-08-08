using System;
using System.Threading;
using System.Threading.Tasks;
using Nanoka.Core.Models;

namespace Nanoka.Core.Client
{
    public class UploadWorker : IDisposable
    {
        readonly CancellationTokenSource _backgroundTaskToken = new CancellationTokenSource();

        readonly IDatabaseClient _client;
        readonly Guid _id;

        internal UploadWorker(IDatabaseClient client, Guid id, UploadState initial)
        {
            _client = client;
            _id     = id;

            State = initial;

            Task.Run(() => RunRefreshAsync(_backgroundTaskToken.Token));
        }

        public UploadState State { get; private set; }

        public event Func<UploadState, CancellationToken, Task> StateUpdatedAsync;

        async Task RunRefreshAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var current = await _client.GetUploadStateAsync(_id, cancellationToken);

                    var @event = StateUpdatedAsync;

                    if (current == null)
                        current = new UploadState
                        {
                            IsRunning = false,
                            Progress  = 0,
                            Message   = $"Worker state '{_id}' could not be retrieved."
                        };

                    State = current;

                    if (@event != null)
                        await @event(current, cancellationToken);

                    if (!current.IsRunning)
                        return;

                    await Task.Delay(TimeSpan.FromSeconds(0.5), cancellationToken);
                }
            }
            catch
            {
                // ignored
            }
        }

        public void Dispose()
        {
            _backgroundTaskToken.Cancel();
            _backgroundTaskToken.Dispose();
        }
    }
}