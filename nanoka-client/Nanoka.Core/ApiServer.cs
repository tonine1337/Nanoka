using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;

namespace Nanoka.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Super lightweight HTTP API server.
    /// </summary>
    public class ApiServer : IDisposable
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        readonly NanokaOptions _options;
        readonly HttpListener _listener;
        readonly JsonSerializer _serializer = JsonSerializer.CreateDefault();

        public ApiServer(NanokaOptions options)
        {
            _options = options;

            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://{options.NanokaEndpoint}");
        }

        readonly Dictionary<Type, Func<object>> _services = new Dictionary<Type, Func<object>>();

        public ApiServer AddService(object obj)
        {
            _services[obj.GetType()] = () => obj;
            return this;
        }

        public ApiServer AddService<T>(T obj)
        {
            _services[typeof(T)] = () => obj;
            return this;
        }

        public ApiServer AddService<T>()
        {
            var ctor = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                                .OrderByDescending(c => c.GetParameters().Length)
                                .FirstOrDefault();

            if (ctor == null)
                throw new NotSupportedException($"Service '{typeof(T)}' does not define a constructor.");

            var paramTypes = ctor.GetParameters().Select(c => c.ParameterType).ToArray();

            _services[typeof(T)] = () => (T) ctor.Invoke(ResolveServices(paramTypes));

            return this;
        }

        object[] ResolveServices(IEnumerable<Type> types, Func<Type, object> func = null)
        {
            var services = new List<object>();

            foreach (var type in types)
            {
                var obj = func?.Invoke(type);

                if (obj != null)
                {
                    services.Add(obj);
                    continue;
                }

                if (_services.TryGetValue(type, out var factory))
                {
                    services.Add(factory());
                    continue;
                }

                throw new NotSupportedException($"Service '{type.Name}' could not be resolved.");
            }

            return services.ToArray();
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            // start listener
            _listener.Start();

            var listeners = new HashSet<Task>();

            async Task listenAsync()
            {
                try
                {
                    // asynchronously wait to get a context
                    var context = await _listener.GetContextAsync();

                    // start a new listener
                    _ = Task.Run(listenAsync, cancellationToken);

                    try
                    {
                        // enter pipeline
                        await HandleContextAsync(context, cancellationToken);
                    }
                    finally
                    {
                        // always close response
                        context.Response.Close();
                    }
                }
                catch (TaskCanceledException)
                {
                    // server shutdown
                }
                catch (Exception e)
                {
                    _log.Debug(e, "Unhandled exception while handling HTTP request.");
                }
            }

            // start initial listeners
            for (var i = 0; i < _options.NanokaServerListeners; i++)
            {
                lock (listeners)
                    listeners.Add(listenAsync());
            }

            try
            {
                await Task.Delay(-1, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // stop listener on cancel
                _listener.Stop();

                throw;
            }
        }

        async Task HandleContextAsync(HttpListenerContext context, CancellationToken cancellationToken = default)
        {
            var request = context.Request;
            var path    = request.Url.AbsolutePath;

            switch (request.HttpMethod.ToUpperInvariant())
            {
                // index page
                case "GET" when path == "/": break;

                // static assets
                case "GET" when path.StartsWith("/static/"): break;

                // ipfs access
                case "GET" when path.StartsWith("/api/fs/"): break;

                // api callback
                case "POST" when path.StartsWith("/api/"):
                    await HandleApiCallback(context, cancellationToken);
                    break;

                default:
                    await RespondAsync(context, HttpStatusCode.NotFound, $"No handler for path '{path}'.");
                    break;
            }
        }

        struct ApiHandlerInfo
        {
            public readonly ConstructorInfo Constructor;
            public readonly Type[] ParamTypes;

            public ApiHandlerInfo(Type type)
            {
                Constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                                  .OrderByDescending(c => c.GetParameters().Length)
                                  .FirstOrDefault();

                if (Constructor == null)
                    throw new NotSupportedException($"API handler '{type}' does not define a constructor.");

                ParamTypes = Constructor.GetParameters().Select(p => p.ParameterType).ToArray();
            }
        }

        static readonly IReadOnlyDictionary<string, ApiHandlerInfo> _apiHandlers =
            typeof(NanokaCore).Assembly
                              .GetTypes()
                              .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ApiRequest)))
                              .ToDictionary(GetApiHandlerPath, t => new ApiHandlerInfo(t));

        static string GetApiHandlerPath(Type type) => $"/api/{type.Name.ToLowerInvariant().Replace("request", "")}";

        async Task HandleApiCallback(HttpListenerContext context, CancellationToken cancellationToken = default)
        {
            var request = context.Request;
            var path    = request.Url.AbsolutePath;

            if (!_apiHandlers.TryGetValue(path, out var handlerInfo))
            {
                await RespondAsync(context, HttpStatusCode.NotFound, $"No API handler for path '{path}'.");
                return;
            }

            if (request.ContentType != "application/json")
            {
                await RespondAsync(context, HttpStatusCode.UnsupportedMediaType, "API request must be JSON.");
                return;
            }

            var handler = (ApiRequest) handlerInfo.Constructor.Invoke(ResolveServices(handlerInfo.ParamTypes));

            using (var reader = new StreamReader(request.InputStream))
            using (var bufferedReader = new StringReader(await reader.ReadToEndAsync()))
                _serializer.Populate(bufferedReader, handler);

            handler.Context = context;

            await handler.RunAsync(cancellationToken);
        }

        static async Task RespondAsync(HttpListenerContext context, HttpStatusCode status, string message)
        {
            context.Response.StatusCode        = (int) status;
            context.Response.StatusDescription = status.ToString();

            using (var writer = new StreamWriter(context.Response.OutputStream))
                await writer.WriteAsync(message);
        }

        public void Dispose() { }
    }
}