using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nanoka.Database;
using Nanoka.Storage;

namespace Nanoka
{
    public class SoftDeleteManager : BackgroundService
    {
        readonly NanokaOptions _options;
        readonly INanokaDatabase _db;
        readonly IStorage _storage;
        readonly ILogger<SoftDeleteManager> _logger;

        public SoftDeleteManager(IOptions<NanokaOptions> options, INanokaDatabase db, IStorage storage, ILogger<SoftDeleteManager> logger)
        {
            _options = options.Value;
            _db      = db;
            _storage = storage;
            _logger  = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var filenames = await _db.GetAndRemoveDeleteFilesAsync(DateTime.UtcNow.AddMilliseconds(-_options.SoftDeleteDelayMs), stoppingToken);

                foreach (var filename in filenames)
                    await _storage.DeleteAsync(filename, stoppingToken);

                _logger.LogInformation($"Hard deleted files: {string.Join(", ", filenames)}");

                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        public async Task DeleteAsync(IEnumerable<string> filenames, CancellationToken cancellationToken = default)
        {
            filenames = filenames.ToArray();

            if (_options.EnableSoftDelete)
            {
                await _db.AddDeleteFilesAsync(filenames, DateTime.UtcNow, cancellationToken);

                _logger.LogInformation($"Soft deleted files: {string.Join(", ", filenames)}");
            }
            else
            {
                foreach (var filename in filenames)
                    await _storage.DeleteAsync(filename, cancellationToken);

                _logger.LogInformation($"Hard deleted files: {string.Join(", ", filenames)}");
            }
        }

        public async Task RestoreAsync(IEnumerable<string> filenames, CancellationToken cancellationToken = default)
        {
            filenames = filenames.ToArray();

            // this won't do much if the file was already hard-deleted
            await _db.RemoveDeleteFileAsync(filenames, cancellationToken);

            _logger.LogInformation($"Restored soft deleted files: {string.Join(", ", filenames)}");
        }
    }
}