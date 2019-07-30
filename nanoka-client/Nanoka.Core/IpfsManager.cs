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

namespace Nanoka.Core
{
    public static class IpfsManager
    {
        /// <summary>
        /// Gets the path to IPFS executable, ensuring that the file exists.
        /// </summary>
        static string GetIpfsPath()
        {
            // ipfs.exe on windows
            // ipfs on any other OS
            var filename = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ipfs.exe" : "ipfs";

            // absolute path
            filename = Path.Combine(Environment.CurrentDirectory, filename);

            if (!File.Exists(filename))
                throw new IpfsManagerException($"Could not find IPFS client in '{filename}'.");

            return filename;
        }

        static Process StartIpfs(string args)
        {
            var process = new ProcessStartInfo
            {
                FileName        = GetIpfsPath(),
                Arguments       = args,
                CreateNoWindow  = true,
                UseShellExecute = false
            };

            return Process.Start(process);
        }

        static void SetConfig(string key, string value)
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

                switch (IPAddress.Parse(address).AddressFamily)
                {
                    case AddressFamily.InterNetwork:   return $"/ip4/{address}/tcp/{port}";
                    case AddressFamily.InterNetworkV6: return $"/ip6/{address}/tcp/{port}";

                    default: throw new NotSupportedException();
                }
            }
            catch (Exception e)
            {
                throw new IpfsManagerException($"Could not convert endpoint '{endpoint}' to multiaddr.", e);
            }
        }

        public static async Task<IpfsClient> StartDaemonAsync(NanokaOptions options,
                                                              CancellationToken cancellationToken = default)
        {
            // update configuration
            SetConfig("Addresses.API", EndpointToMultiAddr(options.IpfsApiEndpoint));
            SetConfig("Addresses.Gateway", EndpointToMultiAddr(options.IpfsGatewayEndpoint));

            // create client
            var client = new IpfsClient($"http://{options.IpfsApiEndpoint}/");

            // start daemon process
            using (var process = StartIpfs($"daemon {options.IpfsDaemonFlags}"))
            {
                var watch = Stopwatch.StartNew();

                while (!process.HasExited)
                {
                    try
                    {
                        // test send request to daemon
                        await TestDaemonAsync(client.FileSystem, cancellationToken);

                        // success
                        return client;
                    }
                    catch (HttpRequestException)
                    {
                        // request could not be sent, so wait
                        if (watch.Elapsed.Seconds < options.IpfsDaemonWaitTimeout)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
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
                                                       $"{options.IpfsDaemonWaitTimeout} seconds.",
                                                       exception);
                    }
                }

                throw new IpfsManagerException($"IPFS daemon exited with code {process.ExitCode}.");
            }
        }

        static async Task TestDaemonAsync(IFileSystemApi fs, CancellationToken cancellationToken = default)
        {
            // this file has the content "ipfs"
            const string ping = "QmejvEPop4D7YUadeGqYWmZxHhLc4JBUCzJJHWMzdcMe2y";

            using (var stream = await fs.GetAsync(ping, false, cancellationToken))
            using (var reader = new StreamReader(stream))
            {
                if (await reader.ReadToEndAsync() != "ipfs")
                    throw new IpfsManagerException("Daemon returned garbage. " +
                                                   "Ensure that you are running a legitimate IPFS daemon.");
            }
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