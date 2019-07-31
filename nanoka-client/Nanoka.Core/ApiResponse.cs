using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Nanoka.Core
{
    public abstract class ApiResponse
    {
        public static ApiResponse Ok => new StatusCodeResponse(HttpStatusCode.OK);

        [JsonProperty("error")]
        public abstract bool Error { get; }

        public abstract Task ExecuteAsync(HttpListenerContext context, JsonSerializer serializer);
    }

    public class StatusCodeResponse : ApiResponse
    {
        public override bool Error => !(200 <= Status && Status < 300);

        [JsonProperty("status")]
        public int Status { get; }

        [JsonProperty("message")]
        public string Message { get; }

        public StatusCodeResponse(HttpStatusCode status, string message = null)
        {
            Status  = (int) status;
            Message = message ?? status.ToString();
        }

        public override async Task ExecuteAsync(HttpListenerContext context, JsonSerializer serializer)
        {
            context.Response.StatusCode        = Status;
            context.Response.StatusDescription = Message;
            context.Response.ContentType       = "application/json";

            var buffer = new StringWriter();

            serializer.Serialize(buffer, this);

            using (var writer = new StreamWriter(context.Response.OutputStream))
                await writer.WriteAsync(buffer.ToString());
        }
    }

    public class ObjectResponse : ApiResponse
    {
        public override bool Error => false;

        [JsonProperty("body")]
        public object Body { get; }

        public ObjectResponse(object body)
        {
            Body = body;
        }

        public override async Task ExecuteAsync(HttpListenerContext context, JsonSerializer serializer)
        {
            context.Response.StatusCode  = 200;
            context.Response.ContentType = "application/json";

            var buffer = new StringWriter();

            serializer.Serialize(buffer, this);

            using (var writer = new StreamWriter(context.Response.OutputStream))
                await writer.WriteAsync(buffer.ToString());
        }
    }
}