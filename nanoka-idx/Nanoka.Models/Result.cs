using System.Net;
using Newtonsoft.Json;

namespace Nanoka.Models
{
    public class ResultModel<T> where T : class
    {
        [JsonProperty("error")]
        public bool Error => !(200 <= Status && Status < 300);

        [JsonProperty("status")]
        public int Status { get; }

        [JsonProperty("message")]
        public string Message { get; }

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