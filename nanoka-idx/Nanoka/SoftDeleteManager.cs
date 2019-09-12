using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
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

        public SoftDeleteManager(IOptions<NanokaOptions> options, INanokaDatabase db, IStorage storage)
        {
            _options = options.Value;
            _db      = db;
            _storage = storage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var filenames = await _db.GetAndRemoveDeleteFilesAsync(DateTime.UtcNow.AddMilliseconds(-_options.SoftDeleteDelayMs), stoppingToken);

                foreach (var filename in filenames)
                    await _storage.DeleteAsync(filename, stoppingToken);

                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        public async Task DeleteAsync(IEnumerable<string> filenames, CancellationToken cancellationToken = default)
        {
            if (!_options.EnableSoftDelete)
            {
                foreach (var filename in filenames)
                    await _storage.DeleteAsync(filename, cancellationToken);

                return;
            }

            await _db.AddDeleteFilesAsync(filenames, DateTime.UtcNow, cancellationToken);
        }

        public async Task RestoreAsync(IEnumerable<string> filenames, CancellationToken cancellationToken = default)
            => await _db.RemoveDeleteFileAsync(filenames, cancellationToken);
    }
}