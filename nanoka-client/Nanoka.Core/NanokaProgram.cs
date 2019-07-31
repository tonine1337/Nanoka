using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nanoka.Core.Database;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Nanoka.Core
{
    public class NanokaProgram : IDisposable
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        static NanokaProgram()
        {
            // initialize logging
            var config = new LoggingConfiguration();
            var target = new ColoredConsoleTarget("target1")
            {
                Layout = @"${date:format=HH\:mm\:ss} ${level} ${logger:shortName=true} ${message} ${exception}"
            };

            config.AddTarget(target);
            config.AddRuleForAllLevels(target);

            LogManager.Configuration = config;

            // required for SQLite
            SQLitePCL.Batteries_V2.Init();
        }

        readonly JsonSerializer _serializer;

        public NanokaProgram(JsonSerializer serializer = null)
        {
            _serializer = serializer ?? JsonSerializer.CreateDefault();
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            using (var server = new ApiServer(await NanokaOptions.LoadAsync(_serializer)))
            {
                // services
                await ConfigureServicesAsync(server, cancellationToken);

                // migrate database
                using (var db = server.ResolveService<NanokaDbContext>())
                {
                    _log.Info("Migrating database...");

                    await db.Database.MigrateAsync(cancellationToken);
                }

                // run server
                await server.RunAsync(cancellationToken);
            }
        }

        async Task ConfigureServicesAsync(ApiServer server, CancellationToken cancellationToken = default)
        {
            var options = server.ResolveService<NanokaOptions>();

            // json serializer
            server.AddService(_serializer);

            // ipfs client
            server.AddService(await IpfsManager.StartDaemonAsync(options, cancellationToken));

            // database
            server.AddService(() =>
            {
                var builder = new DbContextOptionsBuilder().UseSqlite(options.LocalConnectionString);

                return new NanokaDbContext(builder.Options);
            });
        }

        public void Dispose() { }
    }
}