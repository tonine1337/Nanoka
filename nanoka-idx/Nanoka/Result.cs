using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Nanoka
{
    public class Result : Result<object>
    {
        [JsonIgnore]
        public Exception Exception => new ResultException(this);

        Result(HttpStatusCode status, string message) : base(status, message, null) { }

        public static Result StatusCode(HttpStatusCode status, string message) => new Result(status, message);

        public static Result Ok(string message = null) => StatusCode(HttpStatusCode.OK, message);
        public static Result NotFound(string message = null) => StatusCode(HttpStatusCode.NotFound, message);
        public static Result Forbidden(string message = null) => StatusCode(HttpStatusCode.Forbidden, message);
        public static Result BadRequest(string message = null) => StatusCode(HttpStatusCode.BadRequest, message);
        public static Result InternalServerError(string message = null) => StatusCode(HttpStatusCode.InternalServerError, message);
        public static Result UnprocessableEntity(string message = null) => StatusCode(HttpStatusCode.UnprocessableEntity, message);
        public static Result Conflict(string message = null) => StatusCode(HttpStatusCode.Conflict, message);
        public static Result TooManyRequests(string message = null) => StatusCode(HttpStatusCode.TooManyRequests, message);

#region Helpers

        public static Result NotFound<T>(params object[] path)
            => NotFound($"{typeof(T).Name} '{string.Join('/', path)}' not found.");

#endregion
    }

    public class Result<T> : IActionResult
        where T : class
    {
        [JsonProperty("error")]
        public bool Error => !(200 <= Status && Status < 300);

        [JsonProperty("status")]
        public int Status { get; }

        [JsonProperty("message")]
        public string Message { get; }

        /// <summary>
        /// This property exists for unit testing purposes.
        /// </summary>
        [JsonIgnore]
        public object Value { get; }

        public Result(HttpStatusCode status, string message, T value)
        {
            Value = value;

            Status  = (int) status;
            Message = message ?? status.ToString();
        }

        public static implicit operator Result<T>(T value) => new Result<T>(HttpStatusCode.OK, null, value);
        public static implicit operator Result<T>(Result result) => new Result<T>((HttpStatusCode) result.Status, result.Message, null);

        public static implicit operator ActionResult(Result<T> result)
            => new ObjectResult(result.Value ?? result)
            {
                StatusCode = result.Status
            };

        public Task ExecuteResultAsync(ActionContext context)
            => ((ActionResult) this).ExecuteResultAsync(context);
    }
}