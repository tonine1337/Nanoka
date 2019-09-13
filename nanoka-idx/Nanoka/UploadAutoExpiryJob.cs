using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nanoka
{
    public class UploadAutoExpiryJob : BackgroundService
    {
        readonly NanokaOptions _options;
        readonly UploadTaskCollection _tasks;
        readonly ILogger<UploadAutoExpiryJob> _logger;

        public UploadAutoExpiryJob(IOptions<NanokaOptions> options, UploadTaskCollection tasks, ILogger<UploadAutoExpiryJob> logger)
        {
            _options = options.Value;
            _tasks   = tasks;
            _logger  = logger;
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
                {
                    try
                    {
                        task.Dispose(); // disposal is expensive so run outside lock

                        _logger.LogInformation($"Upload worker {task.Id} expired after inactivity.");
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, $"Failed to dispose upload worker {task.Id}'.");
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}