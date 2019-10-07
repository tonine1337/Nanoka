using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Nanoka.Storage
{
    public class SoftDeleteStorage : StorageWrapper
    {
        readonly SoftDeleteOptions _options;
        readonly ISoftDeleteQueue _queue;
        readonly ILogger<SoftDeleteStorage> _logger;

        readonly BackgroundDeleter _deleter;

        public SoftDeleteStorage(IServiceProvider services, IConfiguration configuration, ISoftDeleteQueue queue, ILogger<SoftDeleteStorage> logger)
            : base(services, configuration.GetSection("Storage"))
        {
            _options = configuration.Get<SoftDeleteOptions>();
            _queue   = queue;
            _logger  = logger;
            _deleter = new BackgroundDeleter(this);

            Task.Run(() => _deleter.StartAsync(default));
        }

        public override async Task DeleteAsync(string[] names, CancellationToken cancellationToken = default)
        {
            await _queue.EnqueueAsync(names, DateTime.UtcNow, cancellationToken);

            _logger.LogInformation($"Soft deleted files: {string.Join(", ", names)}");
        }

        // this won't do anything if the file was already hard-deleted
        public override async Task RestoreAsync(string[] names, CancellationToken cancellationToken = default)
        {
            await _queue.DequeueAsync(names, cancellationToken);

            // implementation might also support soft delete
            await base.RestoreAsync(names, cancellationToken);

            _logger.LogInformation($"Restored soft deleted files: {string.Join(", ", names)}");
        }

        public override void Dispose()
        {
            _deleter.Dispose();

            base.Dispose();
        }

        sealed class BackgroundDeleter : BackgroundService
        {
            readonly SoftDeleteStorage _s;

            public BackgroundDeleter(SoftDeleteStorage s)
            {
                _s = s;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var names = await _s._queue.DequeueAsync(DateTime.UtcNow.AddMilliseconds(-_s._options.WaitMs), stoppingToken);

                    if (names.Length != 0)
                    {
                        await _s.Implementation.DeleteAsync(names, stoppingToken);

                        _s._logger.LogInformation($"Hard deleted files: {string.Join(", ", names)}");
                    }

                    await Task.Delay(TimeSpan.FromMinutes(_s._options.CheckIntervalMin), stoppingToken);
                }
            }
        }
    }

    public class SoftDeleteQueue : ISoftDeleteQueue
    {
        readonly HashSet<Item> _set = new HashSet<Item>();

        struct Item
        {
            public readonly string Name;
            public readonly DateTime DeletedTime;

            public Item(string name, DateTime deletedTime)
            {
                Name        = name;
                DeletedTime = deletedTime;
            }

            public override bool Equals(object obj) => obj is Item item && Name == item.Name;
            public override int GetHashCode() => Name?.GetHashCode() ?? 0;
        }

        public Task EnqueueAsync(IEnumerable<string> names, DateTime deletedTime, CancellationToken cancellationToken = default)
        {
            lock (_set)
            {
                foreach (var name in names)
                    _set.Add(new Item(name, deletedTime));
            }

            return Task.CompletedTask;
        }

        public Task DequeueAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
        {
            lock (_set)
            {
                foreach (var name in names)
                    _set.Remove(new Item(name, default));
            }

            return Task.CompletedTask;
        }

        public Task<string[]> DequeueAsync(DateTime maxDeletedTime, CancellationToken cancellationToken = default)
        {
            var list = new List<string>();

            lock (_set)
            {
                foreach (var item in _set)
                {
                    if (item.DeletedTime < maxDeletedTime)
                    {
                        _set.Remove(item);
                        list.Add(item.Name);
                    }
                }
            }

            return Task.FromResult(list.ToArray());
        }
    }
}