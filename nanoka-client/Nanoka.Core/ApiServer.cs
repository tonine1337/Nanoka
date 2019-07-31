using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            _listener.Prefixes.Add($"http://{options.NanokaEndpoint}/");
        }

        readonly Dictionary<Type, Func<object>> _services = new Dictionary<Type, Func<object>>();

        public ApiServer AddService(object obj)
        {
            _services[obj.GetType()] = () => obj;

            _log.Debug($"Service registered: {obj.GetType()}");

            return this;
        }

        public ApiServer AddService<T>(T obj)
        {
            _services[typeof(T)] = () => obj;

            _log.Debug($"Service registered: {obj.GetType()} as {typeof(T)}");

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

            _services[typeof(T)] = delegate
            {
                var obj = (T) ctor.Invoke(ResolveServices(paramTypes));

                _log.Debug($"Transient service constructed: {typeof(T)}");

                return obj;
            };

            _log.Debug($"Transient service registered: {typeof(T)}");

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

                throw new NotSupportedException($"Service '{type}' could not be resolved.");
            }

            return services.ToArray();
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            // start listener
            _listener.Start();

            _log.Info("Started API server.");

            var listeners     = new HashSet<Task>();
            var listenerCount = 0;

            async Task listenAsync()
            {
                try
                {
                    var id = Interlocked.Increment(ref listenerCount);

                    _log.Trace($"Starting listener {id}");

                    // asynchronously wait to get a context
                    var context = await _listener.GetContextAsync();

                    // start a new listener
                    _ = Task.Run(listenAsync, cancellationToken);

                    _log.Trace($"Listener {id} processing request {context.Request.RequestTraceIdentifier}");

                    var watch = Stopwatch.StartNew();

                    try
                    {
                        // enter pipeline
                        await HandleContextAsync(context, cancellationToken);
                    }
                    finally
                    {
                        // always close response
                        context.Response.Close();

                        _log.Trace($"Request {id} finished processing in {watch.Elapsed.Seconds:F} seconds.");
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
                case "GET" when path == "/":
                    await HandleStaticFileAsync(context, "index.html", cancellationToken);
                    break;

                // static assets
                case "GET" when path.StartsWith("/static/"):
                    await HandleStaticFileAsync(context, path.Substring(1), cancellationToken);
                    break;

                // ipfs access
                case "GET" when path.StartsWith("/api/fs/"): break;

                // api callback
                case "POST" when path.StartsWith("/api/"):
                    await HandleApiCallbackAsync(context, path.Substring("/api/".Length), cancellationToken);
                    break;

                default:
                    await RespondAsync(context, HttpStatusCode.NotFound, $"No handler for path '{path}'.");
                    break;
            }
        }

        async Task HandleStaticFileAsync(HttpListenerContext context,
                                         string path,
                                         CancellationToken cancellationToken = default)
        {
            // make absolute
            var fullPath = Path.Combine(Environment.CurrentDirectory, "www", path);

            if (!File.Exists(fullPath))
            {
                await RespondAsync(context, HttpStatusCode.NotFound, $"File '{path}' not found.");
                return;
            }

            context.Response.StatusCode = 200;

            using (var source = File.OpenRead(fullPath))
            using (var destination = context.Response.OutputStream)
                await source.CopyToAsync(destination, 81920, cancellationToken);
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
                              .ToDictionary(t => t.Name.ToLowerInvariant().Replace("request", ""),
                                            t => new ApiHandlerInfo(t));

        async Task HandleApiCallbackAsync(HttpListenerContext context,
                                          string path,
                                          CancellationToken cancellationToken = default)
        {
            if (!_apiHandlers.TryGetValue(path, out var handlerInfo))
            {
                await RespondAsync(context, HttpStatusCode.NotFound, $"No API handler for path '{path}'.");
                return;
            }

            if (context.Request.ContentType != "application/json")
            {
                await RespondAsync(context, HttpStatusCode.UnsupportedMediaType, "API request must be JSON.");
                return;
            }

            var handler = (ApiRequest) handlerInfo.Constructor.Invoke(ResolveServices(handlerInfo.ParamTypes));

            using (var reader = new StreamReader(context.Request.InputStream))
            using (var bufferedReader = new StringReader(await reader.ReadToEndAsync()))
                _serializer.Populate(bufferedReader, handler);

            handler.Context = context;

            try
            {
                var response = await handler.RunAsync(cancellationToken) ?? ApiResponse.Ok;

                await response.ExecuteAsync(context, _serializer);
            }
            catch (Exception e)
            {
                _log.Warn(e);

                await RespondAsync(context, HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        Task RespondAsync(HttpListenerContext context, HttpStatusCode status, string message)
            => new StatusCodeResponse(status, message).ExecuteAsync(context, _serializer);

        public void Dispose() { }
    }
}