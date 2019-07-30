using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Nanoka.Core
{
    public abstract class ApiRequest
    {
        [JsonIgnore]
        public HttpListenerContext Context { get; internal set; }

        public abstract Task RunAsync(CancellationToken cancellationToken = default);
    }
}