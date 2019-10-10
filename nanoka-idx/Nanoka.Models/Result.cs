using System.Net;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class ResultModel<T> where T : class
    {
        /// <summary>
        /// Whether this result represents an error.
        /// </summary>
        [JsonProperty("error")]
        public bool Error => !(200 <= Status && Status < 300);

        /// <summary>
        /// HTTP status code.
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; }

        /// <summary>
        /// Result message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; }

        /// <summary>
        /// Result value.
        /// </summary>
        [JsonProperty("value")]
        public T Value { get; }

        public ResultModel(HttpStatusCode status, string message, T value)
        {
            Value = value;

            Status  = (int) status;
            Message = message ?? status.ToString();
        }
    }
}