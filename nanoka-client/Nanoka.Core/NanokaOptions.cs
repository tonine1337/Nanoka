using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Nanoka.Core
{
    public class NanokaOptions
    {
        public static async Task<NanokaOptions> LoadAsync(JsonSerializer serializer,
                                                          string path = "nanoka.json")
        {
            if (!File.Exists(path))
                return new NanokaOptions();

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
            }
        }

        public string NanokaEndpoint { get; set; } = "localhost:7230";
        public int NanokaServerListeners { get; set; } = 10;

        public string IpfsApiEndpoint { get; set; } = "localhost:5001";
        public string IpfsGatewayEndpoint { get; set; } = "localhost:8080";
        public string IpfsDaemonFlags { get; set; } = "--init --migrate --enable-gc --writable";
        public double IpfsDaemonWaitTimeout { get; set; } = 10;
    }
}