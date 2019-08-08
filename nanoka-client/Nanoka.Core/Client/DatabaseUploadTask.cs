using System;
using System.Threading;
using System.Threading.Tasks;
using Nanoka.Core.Models;

namespace Nanoka.Core.Client
{
    public class DatabaseUploadTask<T> : IDisposable
    {
        readonly CancellationTokenSource _backgroundTaskToken = new CancellationTokenSource();

        readonly IDatabaseClient _client;
        readonly Guid _id;

        internal DatabaseUploadTask(IDatabaseClient client, UploadState<T> state)
        {
            _client = client;
            _id     = state.Id;

            if (state.IsRunning)
                Task.Run(() => RunRefreshAsync(_backgroundTaskToken.Token));
        }

        public UploadState<T> State { get; private set; }

        /// <summary>
        /// Triggered when <see cref="State"/> is changed.
        /// </summary>
        public event Func<UploadState<T>, CancellationToken, Task> StateUpdatedAsync;

        /// <summary>
        /// Periodically refreshes <see cref="State"/> with the latest upload status.
        /// </summary>
        async Task RunRefreshAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var current = await _client.GetUploadStateAsync<T>(_id, cancellationToken);

                    var @event = StateUpdatedAsync;

                    if (current == null)
                        current = new UploadState<T>
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