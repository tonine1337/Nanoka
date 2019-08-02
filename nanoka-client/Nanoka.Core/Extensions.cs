using System.IO;
using Newtonsoft.Json;

namespace Nanoka.Core
{
    public static class Extensions
    {
        public static T Deserialize<T>(this JsonSerializer serializer, TextReader reader)
        {
            using (var jsonReader = new JsonTextReader(reader))
                return serializer.Deserialize<T>(jsonReader);
        }
    }
}