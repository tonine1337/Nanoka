namespace Nanoka.Core
{
    public class NanokaOptions
    {
        public string DatabaseEndpoint { get; set; } = "https://nanoka-idx.chiya.dev";
    }

    public class IpfsOptions
    {
        public string ApiEndpoint { get; set; } = "localhost:5001";
        public string GatewayEndpoint { get; set; } = "localhost:8080";
        public string DaemonFlags { get; set; } = "--init --migrate --enable-gc --writable";
        public double DaemonWaitTimeout { get; set; } = 10;
    }
}