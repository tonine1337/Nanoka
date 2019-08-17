using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Nanoka
{
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

        public UploadWorker CreateWorker(Guid id)
        {
            var worker = new UploadWorker(id, _services);

            lock (_workers)
                _workers[id] = worker;

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
                        // make finished worker information available for longer
                        if (worker.End == null || DateTime.UtcNow < worker.End.Value.AddMinutes(10))
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