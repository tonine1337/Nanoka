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
        readonly object _lock = new object();
        readonly Dictionary<Guid, UploadTask> _tasks = new Dictionary<Guid, UploadTask>();

        public UploadTask AddTask(UploadTask task)
        {
            lock (_tasks)
                _tasks[task.Id] = task;

            return task;
        }

        public UploadTask GetTask(Guid id)
        {
            lock (_lock)
                return _tasks.GetValueOrDefault(id);
        }

        public void RemoveTask(UploadTask task)
        {
            lock (_lock)
            {
                if (_tasks.ContainsKey(task.Id))
                {
                    _tasks.Remove(task.Id);

                    task.Dispose();
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

                var time = DateTime.UtcNow;

                lock (_lock)
                {
                    // find tasks that haven't been updated in a minute
                    foreach (var task in _tasks.Values.Where(t => t.UpdateTime.AddMinutes(1) <= time).ToArray())
                        RemoveTask(task);
                }
            }
        }
    }
}