using Newtonsoft.Json;

namespace Nanoka.Models
{
    public struct FilterQuery<T> where T : struct
    {
        [JsonProperty("value")]
        public T? Value { get; set; }

        [JsonIgnore]
        public bool IsSpecified => Value != null;

        public static implicit operator FilterQuery<T>(T? value) => new FilterQuery<T>
        {
            Value = value
        };
    }
}