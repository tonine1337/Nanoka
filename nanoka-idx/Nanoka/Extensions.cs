using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Nanoka
{
    public static class Extensions
    {
        // https://kristian.hellang.com/using-mvc-result-executors-in-middleware/
        static readonly RouteData _emptyRouteData = new RouteData();
        static readonly ActionDescriptor _emptyActionDescriptor = new ActionDescriptor();

        public static Task ExecuteResultAsync<TResult>(this HttpContext context, TResult result) where TResult : IActionResult
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (result == null) throw new ArgumentNullException(nameof(result));

            var executor = context.RequestServices.GetRequiredService<IActionResultExecutor<TResult>>();

            var routeData     = context.GetRouteData() ?? _emptyRouteData;
            var actionContext = new ActionContext(context, routeData, _emptyActionDescriptor);

            return executor.ExecuteAsync(actionContext, result);
        }

        /// <summary>
        /// This is NOT cryptographically secure!!
        /// </summary>
        public static string RandomString(int len)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            var buffer = new char[len];
            var random = new Random();

            for (var i = 0; i < buffer.Length; i++)
                buffer[i] = chars[random.Next(chars.Length)];

            return new string(buffer);
        }

        public static Dictionary<T1, T2[]> RemoveNullValues<T1, T2>(this Dictionary<T1, T2[]> dict)
        {
            var keys = new List<T1>();

            foreach (var (key, value) in dict)
            {
                if (value == null || value.Length == 0)
                    keys.Add(key);
            }

            foreach (var key in keys)
                dict.Remove(key);

            return dict;
        }

        /// <summary>
        /// Generates a practically unique (enough) string that is shorter than base64 encoded UUID.
        /// This uses <see cref="Timestamp"/>.
        /// </summary>
        public static string UniqueString => Convert.ToBase64String(BitConverter.GetBytes(Timestamp))
                                                    .Substring(0, 22)
                                                    .Replace("/", "_")
                                                    .Replace("+", "-");

        static long _lastTimeStamp = DateTime.UtcNow.Ticks;

        /// <summary>
        /// https://stackoverflow.com/a/14369695
        /// </summary>
        public static long Timestamp
        {
            get
            {
                long original,
                     newValue;

                do
                {
                    original = _lastTimeStamp;

                    var now = DateTime.UtcNow.Ticks;

                    newValue = Math.Max(now, original + 1);
                }
                while (Interlocked.CompareExchange(ref _lastTimeStamp, newValue, original) != original);

                return newValue;
            }
        }

        // https://stackoverflow.com/a/11124118
        // Returns the human-readable file size for an arbitrary, 64-bit file size 
        // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
        public static string GetBytesReadable(long i)
        {
            // Get absolute value
            var absolute = i < 0 ? -i : i;
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute >= 0x1000000000000000) // Exabyte
            {
                suffix   = "EB";
                readable = i >> 50;
            }
            else if (absolute >= 0x4000000000000) // Petabyte
            {
                suffix   = "PB";
                readable = i >> 40;
            }
            else if (absolute >= 0x10000000000) // Terabyte
            {
                suffix   = "TB";
                readable = i >> 30;
            }
            else if (absolute >= 0x40000000) // Gigabyte
            {
                suffix   = "GB";
                readable = i >> 20;
            }
            else if (absolute >= 0x100000) // Megabyte
            {
                suffix   = "MB";
                readable = i >> 10;
            }
            else if (absolute >= 0x400) // Kilobyte
            {
                suffix   = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }

            // Divide by 1024 to get fractional value
            readable = readable / 1024;
            // Return formatted number with suffix
            return readable.ToString("0.### ") + suffix;
        }

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

        public static T2[] ToArrayMany<T1, T2>(this IEnumerable<T1> enumerable, Func<T1, IEnumerable<T2>> selector)
            => enumerable.SelectMany(selector).ToArray();

        public static List<T2> ToList<T1, T2>(this IEnumerable<T1> enumerable, Func<T1, T2> selector)
            => enumerable.Select(selector).ToList();

        public static List<T2> ToListMany<T1, T2>(this IEnumerable<T1> enumerable, Func<T1, IEnumerable<T2>> selector)
            => enumerable.SelectMany(selector).ToList();

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