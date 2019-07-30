using System.Threading;
using System.Threading.Tasks;

namespace Nanoka.Core
{
    public static class NanokaCore
    {
        // required for SQLite
        public static void Initialize() => SQLitePCL.Batteries_V2.Init();

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