using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Nanoka.Core
{
    public static class NanokaCore
    {
        public static void Initialize()
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

        public static async Task RunAsync(NanokaOptions options, CancellationToken cancellationToken = default)
        {
            // IPFS client
            var ipfsClient = await IpfsManager.StartDaemonAsync(options, cancellationToken);

            using (var server = new ApiServer(options))
            {
                // dependencies
                server.AddService(ipfsClient);

                // async hosting
                await server.RunAsync(cancellationToken);
            }
        }
    }
}
