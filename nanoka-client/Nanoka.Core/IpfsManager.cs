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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nanoka.Core
{
    public class IpfsManager
    {
        readonly ILogger<IpfsManager> _logger;
        readonly IpfsOptions _options;
        readonly IpfsClient _client;

        public IpfsManager(ILogger<IpfsManager> logger, IOptions<IpfsOptions> options, IpfsClient client)
        {
            _logger  = logger;
            _options = options.Value;
            _client  = client;
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
                    { "IPFS_PATH", GetIpfsRepoPath() }
                }
            };

            _logger.LogDebug($"Executing IPFS with args: {args}");

            return Process.Start(process);
        }

        void InitRepo()
        {
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

        static string EndpointToMultiAddr(string endpoint)
        {
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