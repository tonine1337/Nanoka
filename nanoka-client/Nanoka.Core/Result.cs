using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Nanoka.Core
{
    public class Result : Result<object>
    {
        Result(HttpStatusCode status, string message) : base(status, message, null) { }

        public Result<T> ToGeneric<T>() => new Result<T>((HttpStatusCode) Status, Message, default);

        public static Result StatusCode(HttpStatusCode status, string message) => new Result(status, message);

        public static Result Ok(string message) => StatusCode(HttpStatusCode.OK, message);
        public static Result NotFound(string message) => StatusCode(HttpStatusCode.NotFound, message);
        public static Result Forbidden(string message) => StatusCode(HttpStatusCode.Forbidden, message);
        public static Result BadRequest(string message) => StatusCode(HttpStatusCode.BadRequest, message);
    }

    public class Result<T> : IActionResult
    {
        [JsonProperty]
        public bool Error => !(200 <= Status && Status < 300);

        [JsonProperty]
        public int Status { get; }

        [JsonProperty]
        public string StatusDescription { get; }

        [JsonProperty]
        public string Message { get; }

        [JsonProperty]
        public T Body { get; }

        public Result(HttpStatusCode status, string message, T body)
        {
            Status            = (int) status;
            StatusDescription = status.ToString();
            Message           = message;
            Body              = body;
        }

        public static implicit operator Result<T>(T value) => new Result<T>(HttpStatusCode.OK, null, value);

        public static implicit operator Result<T>(Result result) => result.ToGeneric<T>();

        public static implicit operator ActionResult(Result<T> result)
            => new ObjectResult(result)
            {
                StatusCode = result.Status
            };

        public Task ExecuteResultAsync(ActionContext context) => ((ActionResult) this).ExecuteResultAsync(context);
    }
}