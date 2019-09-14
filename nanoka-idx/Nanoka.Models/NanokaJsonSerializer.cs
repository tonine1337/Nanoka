using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nanoka.Models
{
    public static class NanokaJsonSerializer
    {
        public static JsonSerializerSettings Apply(JsonSerializerSettings settings)
        {
            settings.Converters.Add(new StringEnumConverter());

            return settings;
        }

        public static JsonSerializer Create() => JsonSerializer.Create(Apply(new JsonSerializerSettings()));
    }
}