using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Models;
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

#region Helpers

        public static Result NotFound<T>(params object[] path)
            => NotFound($"{typeof(T).Name} '{string.Join('/', path)}' not found.");

        public static Result Forbidden(params UserPermissions[] required)
            => Forbidden($"Insufficient permissions. Required: {string.Join(", ", required)}");

        public static Result InvalidRecaptchaToken(string token)
            => BadRequest($"Failed reCAPTCHA verification. Token: {token ?? "<not specified>"}");

        public static Result InvalidUpload<T>(params object[] path)
            => BadRequest($"{typeof(T).Name} '{string.Join('/', path)}' is not awaiting upload.");

        public static Result UploadDeleted<T>(params object[] path)
            => Conflict($"{typeof(T).Name} '{string.Join('/', path)}' was deleted before this upload was finalized.");

#endregion
    }

    public class Result<T> : IActionResult
        where T : class
    {
        readonly object _value;

        [JsonProperty("error")]
        public bool Error => !(200 <= Status && Status < 300);

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        public Result(HttpStatusCode status, string message, T value)
        {
            _value = value;

            Status  = (int) status;
            Message = message ?? status.ToString();
        }

        public static implicit operator Result<T>(T value) => new Result<T>(HttpStatusCode.OK, null, value);
        public static implicit operator Result<T>(Result result) => new Result<T>((HttpStatusCode) result.Status, result.Message, null);

        public static implicit operator ActionResult(Result<T> result)
            => new ObjectResult(result._value ?? result)
            {
                StatusCode = result.Status
            };

        public Task ExecuteResultAsync(ActionContext context)
            => ((ActionResult) this).ExecuteResultAsync(context);
    }
}