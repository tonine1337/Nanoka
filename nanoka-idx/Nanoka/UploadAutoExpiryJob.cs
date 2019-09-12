using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Nanoka
{
    public class UploadAutoExpiryJob : BackgroundService
    {
        readonly NanokaOptions _options;
        readonly UploadTaskCollection _tasks;

        public UploadAutoExpiryJob(IOptions<NanokaOptions> options, UploadTaskCollection tasks)
        {
            _options = options.Value;
            _tasks   = tasks;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var time = DateTime.UtcNow;

                UploadTask[] expiredTasks;

                lock (_tasks)
                {
                    expiredTasks = _tasks.Values.Where(t => t.UpdateTime.AddMilliseconds(_options.UploadTaskExpiryMs) <= time).ToArray();

                    foreach (var task in expiredTasks)
                        _tasks.Remove(task.Id);
                }

                foreach (var task in expiredTasks)
                    task.Dispose(); // disposal is expensive so run outside lock

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}