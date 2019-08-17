using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Nanoka
{
    public static class Extensions
    {
        public static void Validate(this object obj) => Validator.ValidateObject(obj, new ValidationContext(obj));

        public static async Task<IDisposable> EnterAsync(this SemaphoreSlim semaphore,
                                                         CancellationToken cancellationToken = default)
        {
            await semaphore.WaitAsync(cancellationToken);
            return new SemaphoreContext(semaphore);
        }

        sealed class SemaphoreContext : IDisposable
        {
            readonly SemaphoreSlim _semaphore;

            public SemaphoreContext(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose() => _semaphore.Release();
        }

        public static string ToShortString(this Guid guid)
            => Convert.ToBase64String(guid.ToByteArray())
                      .Substring(0, 22)
                      .Replace("/", "_")
                      .Replace("+", "-");

        public static Guid ToGuid(this string str)
            => new Guid(Convert.FromBase64String(str.Replace("_", "/").Replace("-", "+") + "=="));

        /// <summary>
        /// https://stackoverflow.com/a/50456283
        /// </summary>
        public static Guid SecureGuid()
        {
            using (var provider = RandomNumberGenerator.Create())
            {
                var bytes = new byte[16];

                provider.GetBytes(bytes);

                bytes[8] = (byte) ((bytes[8] & 0xBF) | 0x80);
                bytes[7] = (byte) ((bytes[7] & 0x4F) | 0x40);

                return new Guid(bytes);
            }
        }

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