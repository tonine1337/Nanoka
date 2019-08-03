using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Nanoka.Core
{
    public static class Extensions
    {
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
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
    }
}