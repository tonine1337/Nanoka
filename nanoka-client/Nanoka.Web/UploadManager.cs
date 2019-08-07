using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nanoka.Core;

namespace Nanoka.Web
{
    public delegate Task UploadWorkerDelegate(IServiceProvider services,
                                              UploadWorker worker,
                                              CancellationToken cancellationToken = default);

    public class UploadManager : BackgroundService
    {
        readonly Dictionary<Guid, UploadWorker> _workers = new Dictionary<Guid, UploadWorker>();

        readonly IServiceProvider _services;

        public UploadManager(IServiceProvider services)
        {
            _services = services;
        }

        /// <summary>
        /// Finds an upload worker by its identifier, returning null if not found.
        /// </summary>
        public UploadWorker FindWorker(Guid id)
        {
            lock (_workers)
                return _workers.GetOrDefault(id);
        }

        public UploadWorker CreateWorker(UploadWorkerDelegate func)
        {
            // create worker object
            var worker = new UploadWorker();

            lock (_workers)
                _workers[worker.Id] = worker;

            // run worker in background
            Task.Run(async () =>
            {
                try
                {
                    using (var scope = _services.CreateScope())
                        await func(scope.ServiceProvider, worker, worker.CancellationToken);

                    // worker function should have set the progress to 1 when it finished
                    if (worker.IsRunning)
                        worker.SetFailure("Upload worker ended prematurely due to an unknown reason.");
                }
                catch (TaskCanceledException)
                {
                    worker.SetFailure("Upload worker has been canceled.");
                }
                catch (Exception e)
                {
                    worker.SetFailure(e.Message);
                }
            });

            return worker;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // periodically prune stopped workers
            while (!stoppingToken.IsCancellationRequested)
            {
                lock (_workers)
                {
                    foreach (var (id, worker) in _workers.ToArray())
                    {
                        if (worker.IsRunning)
                            continue;

                        // make finished worker information available for longer
                        if (DateTime.UtcNow < worker.End.AddMinutes(10))
                            continue;

                        _workers.Remove(id);

                        worker.Dispose();
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            lock (_workers)
            {
                foreach (var (_, worker) in _workers.ToArray())
                    worker.Dispose();

                _workers.Clear();
            }
        }
    }
}