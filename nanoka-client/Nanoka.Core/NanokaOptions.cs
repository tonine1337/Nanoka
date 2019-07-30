namespace Nanoka.Core
{
    public class NanokaOptions
    {
        public string NanokaEndpoint { get; set; } = "localhost:7230";

        public string IpfsApiEndpoint { get; set; } = "localhost:5001";
        public string IpfsGatewayEndpoint { get; set; } = "localhost:8080";
        public string IpfsDaemonFlags { get; set; } = "--init --migrate --enable-gc --writable";
        public double IpfsDaemonWaitTimeout { get; set; } = 10;
    }
}