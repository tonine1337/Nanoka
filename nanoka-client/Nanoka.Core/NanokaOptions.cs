namespace Nanoka.Core
{
    public class NanokaOptions { }

    public class IpfsOptions
    {
        public string ApiEndpoint { get; set; }
        public string GatewayEndpoint { get; set; }
        public string DaemonFlags { get; set; }
        public double DaemonWaitTimeout { get; set; }
    }
}