using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nanoka.Database;

namespace Nanoka.Storage
{
    public class SoftDeleteStorage : StorageWrapper
    {
        readonly SoftDeleteOptions _options;
        readonly INanokaDatabase _db;
        readonly ILogger<SoftDeleteStorage> _logger;

        readonly BackgroundDeleter _deleter;

        public SoftDeleteStorage(IServiceProvider services, IConfiguration configuration, INanokaDatabase db, ILogger<SoftDeleteStorage> logger)
            : base(services, configuration.GetSection("Storage"))
        {
            _options = configuration.Get<SoftDeleteOptions>();
            _db      = db;
            _logger  = logger;
            _deleter = new BackgroundDeleter(this);

            Task.Run(() => _deleter.StartAsync(default));
        }

        public override async Task DeleteAsync(string[] names, CancellationToken cancellationToken = default)
        {
            await _db.AddAsync(names, DateTime.UtcNow, cancellationToken);

            _logger.LogInformation($"Soft deleted files: {string.Join(", ", names)}");
        }

        public override async Task UndeleteAsync(string[] names, CancellationToken cancellationToken = default)
        {
            // this won't do anything if the file was already hard-deleted
            await _db.RemoveAsync(names, cancellationToken);

            // implementation might also support undelete
            await base.UndeleteAsync(names, cancellationToken);

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
                    var filenames = await _s._db.GetAndRemoveAsync(DateTime.UtcNow.AddMilliseconds(-_s._options.WaitMs), stoppingToken);

                    if (filenames.Length != 0)
                    {
                        await _s.Implementation.DeleteAsync(filenames, stoppingToken);

                        _s._logger.LogInformation($"Hard deleted files: {string.Join(", ", filenames)}");
                    }

                    await Task.Delay(TimeSpan.FromMinutes(_s._options.CheckIntervalMin), stoppingToken);
                }
            }
        }
    }
}