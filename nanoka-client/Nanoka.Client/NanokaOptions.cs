using System;

namespace Nanoka.Client
{
    public class NanokaOptions { }

    public class IpfsOptions
    {
        public string ApiEndpoint { get; set; } = "localhost:5001";
        public string GatewayEndpoint { get; set; } = null; // "localhost:8080";
        public string DaemonFlags { get; set; } = "--init --migrate --enable-gc --writable";
        public double DaemonWaitTimeout { get; set; } = 10;
    }

    public class DatabaseOptions
    {
        public string Endpoint { get; set; } = "https://nanoka-idx.chiya.dev";
        public Guid? Token { get; set; }
    }
}