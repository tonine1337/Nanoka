using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ipfs.Http;
using MimeTypes;
using Newtonsoft.Json;
using NLog;

namespace Nanoka.Core
{
    /// <summary>
    /// Super lightweight HTTP API server.
    /// </summary>
    // ReSharper disable once InheritdocConsiderUsage
    public class ApiServer : IDisposable
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        readonly NanokaOptions _options;
        readonly HttpListener _listener;

        public ApiServer(NanokaOptions options)
        {
            _options = options;

            // http
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://{options.NanokaEndpoint}/");

            // always available services
            AddService(this);
            AddService(_listener);
            AddService(options);

            // default services that can be overridden
            AddService(JsonSerializer.CreateDefault());
        }

        readonly Dictionary<Type, ServiceDescriptor> _services = new Dictionary<Type, ServiceDescriptor>();

        enum ServiceType
        {
            Singleton,
            Transient
        }

        struct ServiceDescriptor
        {
            public readonly Func<object> Factory;
            public readonly ServiceType Type;

            public ServiceDescriptor(Func<object> factory, ServiceType type)
            {
                Factory = factory;
                Type    = type;
            }
        }

        public ApiServer AddService(object obj)
        {
            _services[obj.GetType()] = new ServiceDescriptor(() => obj, ServiceType.Singleton);

            _log.Debug($"Service registered: {obj.GetType()}");

            return this;
        }

        public ApiServer AddService<T>(T obj)
        {
            _services[typeof(T)] = new ServiceDescriptor(() => obj, ServiceType.Singleton);

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

            _services[typeof(T)] = new ServiceDescriptor(
                () =>
                {
                    var obj = (T) ctor.Invoke(ResolveServices(paramTypes));

                    _log.Debug($"Transient service constructed: {typeof(T)}");

                    return obj;
                },
                ServiceType.Transient);

            _log.Debug($"Transient service registered: {typeof(T)}");

            return this;
        }

        public T ResolveService<T>(bool required = true)
        {
            if (_services.TryGetValue(typeof(T), out var descriptor))
                return (T) descriptor.Factory();

            if (!required)
                return default;

            throw new NotSupportedException($"Service '{typeof(T)}' could not be resolved.");
        }

        object[] ResolveServices(IEnumerable<Type> types, Func<Type, object> func = null, bool required = true)
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

                if (_services.TryGetValue(type, out var descriptor))
                {
                    services.Add(descriptor.Factory());
                    continue;
                }

                if (required)
                    throw new NotSupportedException($"Service '{type}' could not be resolved.");

                services.Add(null);
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
                    _log.Warn(e, "Unhandled exception while handling HTTP request.");
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

        const string _apiBase = "api/";
        const string _apiFsBase = "api/fs/";

        async Task HandleContextAsync(HttpListenerContext context, CancellationToken cancellationToken = default)
        {
            var request = context.Request;
            var path    = request.Url.AbsolutePath.Substring(1); // trim first slash

            switch (request.HttpMethod.ToUpperInvariant())
            {
                // api callback
                case "POST" when path.StartsWith(_apiBase):
                    await HandleApiCallbackAsync(context, path.Substring(_apiBase.Length), cancellationToken);
                    break;

                // ipfs access
                case "GET" when path.StartsWith(_apiFsBase):
                    await HandleIpfsAccessAsync(context, path.Substring(_apiFsBase.Length), cancellationToken);
                    break;

                // static assets
                case "GET":
                    await HandleStaticFileAsync(context, path, cancellationToken);
                    break;

                default:
                    await RespondAsync(context, HttpStatusCode.NotFound, $"No handler for path '{path}'.");
                    break;
            }
        }

        const int _streamCopyBufferSize = 81920;

        async Task HandleStaticFileAsync(HttpListenerContext context,
                                         string path,
                                         CancellationToken cancellationToken = default)
        {
            // use index.html
            if (path.Length == 0 || path.EndsWith("/"))
                path += "index.html";

            // make absolute
            var fullPath = Path.Combine(Environment.CurrentDirectory, "www", path);

            if (!File.Exists(fullPath))
            {
                await RespondAsync(context, HttpStatusCode.NotFound, $"File '{path}' not found.");
                return;
            }

            context.Response.StatusCode  = 200;
            context.Response.ContentType = MimeTypeMap.GetMimeType(Path.GetExtension(path));

            using (var stream = File.OpenRead(fullPath))
                await stream.CopyToAsync(context.Response.OutputStream, _streamCopyBufferSize, cancellationToken);
        }

        async Task HandleIpfsAccessAsync(HttpListenerContext context,
                                         string path,
                                         CancellationToken cancellationToken = default)
        {
            var client = ResolveService<IpfsClient>(false);

            if (client == null)
            {
                await RespondAsync(context, HttpStatusCode.ServiceUnavailable, "IPFS service is unavailable.");
                return;
            }

            // split name and ext
            var extension   = Path.GetExtension(path) ?? "";
            var contentType = MimeTypeMap.GetMimeType(extension);

            if (!path.Contains("/"))
                path = path.Substring(0, path.Length - extension.Length);

            Stream stream;

            try
            {
                stream = await client.FileSystem.ReadFileAsync(path, cancellationToken);
            }
            catch (Exception e)
            {
                _log.Info(e, "Exception while reading IPFS file.");

                await RespondAsync(context, HttpStatusCode.BadRequest, $"IPFS exception: {e.Message}");
                return;
            }

            using (stream)
            {
                context.Response.StatusCode  = 200;
                context.Response.ContentType = contentType;

                await stream.CopyToAsync(context.Response.OutputStream, _streamCopyBufferSize, cancellationToken);
            }
        }

        struct ApiHandlerInfo
        {
            public readonly ConstructorInfo Ctor;
            public readonly Type[] Params;

            public ApiHandlerInfo(Type type)
            {
                Ctor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                           .OrderByDescending(c => c.GetParameters().Length)
                           .FirstOrDefault();

                if (Ctor == null)
                    throw new NotSupportedException($"API handler '{type}' does not define a constructor.");

                Params = Ctor.GetParameters().Select(p => p.ParameterType).ToArray();
            }
        }

        static readonly IReadOnlyDictionary<string, ApiHandlerInfo> _apiHandlers =
            typeof(NanokaProgram).Assembly
                                 .GetTypes()
                                 .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ApiRequest)))
                                 .ToDictionary(t => t.Name.ToLowerInvariant().Replace("request", ""),
                                               t => new ApiHandlerInfo(t));

        async Task HandleApiCallbackAsync(HttpListenerContext context,
                                          string path,
                                          CancellationToken cancellationToken = default)
        {
            var serializer = ResolveService<JsonSerializer>();

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

            try
            {
                // scoped dependencies
                object getDependency(Type type)
                {
                    // ReSharper disable ConvertIfStatementToReturnStatement
                    if (type == typeof(HttpListenerContext))
                        return context;

                    if (type == typeof(HttpListenerRequest))
                        return context.Request;

                    if (type == typeof(HttpListenerResponse))
                        return context.Response;

                    // ReSharper restore ConvertIfStatementToReturnStatement
                    return null;
                }

                // create handler with dependency injection
                var handler = (ApiRequest) handlerInfo.Ctor.Invoke(ResolveServices(handlerInfo.Params, getDependency));

                using (var reader = new StreamReader(context.Request.InputStream))
                using (var bufferedReader = new StringReader(await reader.ReadToEndAsync()))
                    serializer.Populate(bufferedReader, handler);

                handler.Context = context;

                var response = await handler.RunAsync(cancellationToken) ?? ApiResponse.Ok;

                await response.ExecuteAsync(context, serializer);
            }
            catch (Exception e)
            {
                _log.Warn(e);

                await RespondAsync(context, HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        Task RespondAsync(HttpListenerContext context, HttpStatusCode status, string message)
            => new StatusCodeResponse(status, message).ExecuteAsync(context, ResolveService<JsonSerializer>());

        public void Dispose()
        {
            // dispose services
            foreach (var descriptor in _services.Values)
            {
                if (descriptor.Type != ServiceType.Singleton)
                    continue;

                var service = descriptor.Factory();

                if (service is IDisposable disposable)
                    disposable.Dispose();
            }
        }
    }
}