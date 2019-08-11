using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Ipfs.CoreApi;
using Ipfs.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nanoka.Core;

namespace Nanoka.Client
{
    public class IpfsManager
    {
        readonly ILogger<IpfsManager> _logger;
        readonly IpfsOptions _options;
        readonly IpfsClient _client;
        readonly IHostingEnvironment _hostingEnvironment;

        public IpfsManager(ILogger<IpfsManager> logger,
                           IOptions<IpfsOptions> options,
                           IpfsClient client,
                           IHostingEnvironment hostingEnvironment)
        {
            _logger             = logger;
            _options            = options.Value;
            _client             = client;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Gets the path to IPFS executable, ensuring that the file exists.
        /// </summary>
        static string GetIpfsPath()
        {
            // "ipfs.exe" on windows
            // "ipfs" on any other OS
            var filename = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ipfs.exe" : "ipfs";

            // absolute path
            filename = Path.Combine(Environment.CurrentDirectory, filename);

            if (!File.Exists(filename))
                throw new IpfsManagerException($"Could not find IPFS client in '{filename}'.");

            return filename;
        }

        /// <summary>
        /// Gets the path to IPFS repository directory.
        /// </summary>
        static string GetIpfsRepoPath()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "data");

            Directory.CreateDirectory(path);

            return path;
        }

        Process StartIpfs(string args)
        {
            var process = new ProcessStartInfo
            {
                FileName        = GetIpfsPath(),
                Arguments       = args,
                CreateNoWindow  = true,
                UseShellExecute = false,
                EnvironmentVariables =
                {
                    // custom IPFS path so that it doesn't clash with other installations
                    { "IPFS_PATH", GetIpfsRepoPath() },

                    // completely fail when trying to connect to nodes outside of our private network (misconfiguration)
                    { "LIBP2P_FORCE_PNET", "1" }
                }
            };

            _logger.LogDebug($"Executing IPFS with args: {args}");

            return Process.Start(process);
        }

        void InitRepo()
        {
            // start with a fresh repository in development
            if (!_hostingEnvironment.IsProduction() &&
                Directory.Exists(GetIpfsRepoPath()))
                Directory.Delete(GetIpfsRepoPath(), true);

            // shutdown existing daemon
            using (var process = StartIpfs("shutdown"))
            {
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    _logger.LogInformation("Existing daemon has been shut down.");
                    return;
                }
            }

            // delete residue api file
            File.Delete(Path.Combine(GetIpfsRepoPath(), "api"));

            // initialize repository
            using (var process = StartIpfs("init"))
            {
                process.WaitForExit();

                if (process.ExitCode == 0)
                    _logger.LogInformation($"IPFS repository initialized: {GetIpfsRepoPath()}");
            }
        }

        void SetConfig(string key, string value)
        {
            using (var process = StartIpfs($"config \"{key}\" \"{value}\""))
            {
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new IpfsManagerException($"IPFS client exited with code {process.ExitCode} " +
                                                   $"while updating configuration '{key}' = '{value}'.");
            }
        }

        void SetBootstrap(string addr, string key)
        {
            using (var process = StartIpfs("bootstrap rm --all"))
            {
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new IpfsManagerException($"IPFS client exited with code {process.ExitCode}" +
                                                   "while resetting old bootstrap nodes.");
            }

            using (var process = StartIpfs($"bootstrap add {addr}"))
            {
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new IpfsManagerException($"IPFS client exited with code {process.ExitCode}" +
                                                   $"configuring bootstrap node for '{addr}'.");
            }

            using (var stream = File.Open(Path.Combine(GetIpfsRepoPath(), "swarm.key"), FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
                writer.Write(key);
        }

        static string EndpointToMultiAddr(string endpoint)
        {
            if (endpoint == null)
                return null;

            try
            {
                var delimiter = endpoint.LastIndexOf(':');
                var address   = endpoint.Substring(0, delimiter);
                var port      = ushort.Parse(endpoint.Substring(delimiter + 1));

                // IPAddress.Parse does not handle localhost
                if (address == "localhost")
                    address = "127.0.0.1";

                switch (IPAddress.Parse(address).AddressFamily)
                {
                    case AddressFamily.InterNetwork:   return format(4);
                    case AddressFamily.InterNetworkV6: return format(6);

                    default: throw new NotSupportedException();
                }

                string format(int v) => $"/ip{v}/{address}/tcp/{port}";
            }
            catch (Exception e)
            {
                throw new IpfsManagerException($"Could not convert endpoint '{endpoint}' to multiaddr.", e);
            }
        }

        public async Task StartDaemonAsync(CancellationToken cancellationToken)
        {
            _client.ApiUri = new Uri($"http://{_options.ApiEndpoint}");

            var measure = new MeasureContext();

            // initialize repository
            InitRepo();

            // update configuration
            SetConfig("Addresses.API", EndpointToMultiAddr(_options.ApiEndpoint));
            SetConfig("Addresses.Gateway", EndpointToMultiAddr(_options.GatewayEndpoint));

            SetBootstrap(_options.SwarmBootstrap, _options.SwarmKey);

            // start daemon process
            using (var process = StartIpfs($"daemon {_options.DaemonFlags}"))
            {
                while (!process.HasExited)
                {
                    try
                    {
                        // test send request to daemon
                        await TestDaemonAsync(_client.FileSystem, cancellationToken);

                        // success
                        _logger.LogInformation($"Initialized IPFS daemon in {measure}.");

                        return;
                    }
                    catch (HttpRequestException)
                    {
                        // request could not be sent, so wait
                        if (measure.Watch.Elapsed.Seconds < _options.DaemonWaitTimeout)
                        {
                            await Task.Yield();
                            continue;
                        }

                        // it took too long for the daemon to respond
                        var exception = null as Exception;

                        try
                        {
                            process.Kill();
                        }
                        catch (Exception e)
                        {
                            exception = e;
                        }

                        throw new IpfsManagerException("IPFS daemon failed to respond in " +
                                                       $"{_options.DaemonWaitTimeout} seconds.",
                                                       exception);
                    }
                }

                throw new IpfsManagerException($"IPFS daemon exited with code {process.ExitCode}.");
            }
        }

        async Task TestDaemonAsync(IFileSystemApi fs, CancellationToken cancellationToken = default)
        {
            // this file has the content "ipfs"
            const string ping = "QmejvEPop4D7YUadeGqYWmZxHhLc4JBUCzJJHWMzdcMe2y";

            _logger.LogDebug("Test sending request to daemon...");

            var response = await fs.ReadAllTextAsync(ping, cancellationToken);

            if (response != "ipfs")
                throw new IpfsManagerException("Daemon returned garbage. " +
                                               "Ensure that you are running a legitimate IPFS daemon.");

            _logger.LogDebug("Daemon responded.");
        }
    }

    [Serializable]
    public class IpfsManagerException : Exception
    {
        public IpfsManagerException() { }
        public IpfsManagerException(string message) : base(message) { }
        public IpfsManagerException(string message, Exception inner) : base(message, inner) { }

        protected IpfsManagerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}