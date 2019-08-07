using System;
using System.Threading;
using System.Threading.Tasks;
using Ipfs.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Nanoka.Web
{
    public class IpfsManager : BackgroundService
    {
        public IpfsManager(IOptions<NanokaOptions> options, IpfsClient ipfs)
        {
            ipfs.ApiUri = new Uri(options.Value.IpfsEndpoint);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
    }
}