using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Nanoka.Core
{
    public static class Extensions
    {
        /// <summary>
        /// Registers a background service that can be injected into other services.
        /// </summary>
        public static IServiceCollection AddHostedDependencyService<TService>(this IServiceCollection collection)
            where TService : class, IHostedService
            => collection.AddSingleton<TService>()
                         .AddSingleton<IHostedService, TService>(s => s.GetService<TService>());

        public static string Serialize(this JsonSerializer serializer, object obj)
        {
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        public static T Deserialize<T>(this JsonSerializer serializer, string value)
        {
            using (var reader = new StringReader(value))
                return serializer.Deserialize<T>(reader);
        }

        public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key)
            => dict.TryGetValue(key, out var value) ? value : default;

        public static Dictionary<TKey, TValue[]> BuildArrayDict<TKey, TValue>(params (TKey, TValue[])[] pairs)
        {
            var dict = new Dictionary<TKey, TValue[]>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var (key, values) in pairs)
            {
                if (values != null && values.Length != 0)
                    dict.Add(key, values);
            }

            return dict.Count == 0 ? null : dict;
        }

        public static T2[] ToArray<T1, T2>(this IEnumerable<T1> enumerable, Func<T1, T2> selector)
            => enumerable.Select(selector).ToArray();

        public static List<T2> ToList<T1, T2>(this IEnumerable<T1> enumerable, Func<T1, T2> selector)
            => enumerable.Select(selector).ToList();

        public static T Deserialize<T>(this JsonSerializer serializer, TextReader reader)
        {
            using (var jsonReader = new JsonTextReader(reader))
                return serializer.Deserialize<T>(jsonReader);
        }

        // ReSharper disable once IdentifierTypo : enumerables
        public static IEnumerable<T> IntersectMany<T>(IEnumerable<IEnumerable<T>> enumerables)
        {
            HashSet<T> set = null;

            foreach (var enumerable in enumerables)
            {
                if (set == null)
                    set = new HashSet<T>(enumerable);
                else
                    set.IntersectWith(enumerable);
            }

            return set;
        }
    }
}