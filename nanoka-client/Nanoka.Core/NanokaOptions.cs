using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;

namespace Nanoka.Core
{
    public class NanokaOptions
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public static async Task<NanokaOptions> LoadAsync(JsonSerializer serializer,
                                                          string path = "nanoka.json")
        {
            if (!File.Exists(path))
            {
                _log.Info($"Configuration file '{path}' not found. Using default values.");

                return new NanokaOptions();
            }

            using (var stream = File.OpenRead(path))
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(new StringReader(await reader.ReadToEndAsync())))
                return serializer.Deserialize<NanokaOptions>(jsonReader);
        }

        public static async Task SaveAsync(JsonSerializer serializer,
                                           NanokaOptions options,
                                           string path = "nanoka.json")
        {
            using (var stream = File.OpenWrite(path))
            using (var writer = new StreamWriter(stream))
            {
                var buffer = new StringWriter();

                serializer.Serialize(buffer, options);

                await writer.WriteAsync(buffer.ToString());

                _log.Info($"Saved configuration at '{path}'.");
            }
        }

        public string NanokaEndpoint { get; set; } = "localhost:7230";
        public int HttpListenConcurrency { get; set; } = 10;

        public string LocalConnectionString { get; set; } = "Data Source=nanoka.db;";

        public string IpfsApiEndpoint { get; set; } = "localhost:5001";
        public string IpfsGatewayEndpoint { get; set; } = "localhost:8080";
        public string IpfsDaemonFlags { get; set; } = "--init --migrate --enable-gc --writable";
        public double IpfsDaemonWaitTimeout { get; set; } = 10;
    }
}