using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Core.Models;
using Newtonsoft.Json;

namespace Nanoka.Core
{
    public class Result : Result<object>
    {
        Result(HttpStatusCode status, string message) : base(status, message, null) { }

        public Result<T> ToGeneric<T>() => new Result<T>((HttpStatusCode) Status, Message, default);

        public static Result StatusCode(HttpStatusCode status, string message) => new Result(status, message);

        public static Result Ok(string message = null) => StatusCode(HttpStatusCode.OK, message);
        public static Result NotFound(string message = null) => StatusCode(HttpStatusCode.NotFound, message);
        public static Result Forbidden(string message = null) => StatusCode(HttpStatusCode.Forbidden, message);
        public static Result BadRequest(string message = null) => StatusCode(HttpStatusCode.BadRequest, message);

#region Helpers

        public static Result NotFound<T>(object id) => NotFound($"{typeof(T).Name} '{id}' not found.");

        public static Result Forbidden(params UserPermissions[] required)
            => Forbidden($"Insufficient permissions. Required: {string.Join(", ", required)}");

#endregion
    }

    public class Result<T> : IActionResult
    {
        [JsonProperty("error")]
        public bool Error => !(200 <= Status && Status < 300);

        [JsonProperty("status")]
        public int Status { get; }

        [JsonProperty("message")]
        public string Message { get; }

        [JsonProperty("body")]
        public T Body { get; }

        public Result(HttpStatusCode status, string message, T body)
        {
            Status  = (int) status;
            Message = message ?? status.ToString();
            Body    = body;
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