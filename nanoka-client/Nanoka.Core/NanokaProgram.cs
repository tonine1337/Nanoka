using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Nanoka.Core
{
    public class NanokaProgram : IDisposable
    {
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
            // load config
            var options = await NanokaOptions.LoadAsync(_serializer);

            // ipfs client
            var ipfsClient = await IpfsManager.StartDaemonAsync(options, cancellationToken);

            // api server
            using (var server = new ApiServer(options))
            {
                // dependency registration
                server.AddService(_serializer)
                      .AddService(ipfsClient);

                await server.RunAsync(cancellationToken);
            }
        }

        public void Dispose() { }
    }
}