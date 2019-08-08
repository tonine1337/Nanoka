using System.Net.Http;
using Ipfs.Http;
using Microsoft.Extensions.Options;
using Nanoka.Core.Client;
using Newtonsoft.Json;

namespace Nanoka.Client
{
    public class DatabaseClient : DatabaseClientBase
    {
        public DatabaseClient(IOptions<DatabaseOptions> options,
                              JsonSerializer serializer,
                              IHttpClientFactory httpClientFactory,
                              IpfsClient ipfs)
            : base(options.Value.Endpoint,
                   options.Value.Secret,
                   serializer,
                   httpClientFactory.CreateClient(nameof(DatabaseClientBase)),
                   ipfs) { }
    }
}