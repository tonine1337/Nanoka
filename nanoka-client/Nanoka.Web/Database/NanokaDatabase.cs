using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nanoka.Core.Models;
using Nest;

namespace Nanoka.Web.Database
{
    public class NanokaDatabase
    {
        readonly ElasticClient _client;

        readonly NanokaOptions _options;
        readonly ILogger<NanokaDatabase> _logger;

        public NanokaDatabase(IOptions<NanokaOptions> options, ILogger<NanokaDatabase> logger)
        {
            _options = options.Value;
            _logger  = logger;

            if (_options.ElasticEndpoint == null)
                throw new NanokaDatabaseException("Elasticsearch endpoint is not configured.");

            var pool       = new SingleNodeConnectionPool(new Uri(_options.ElasticEndpoint));
            var connection = new ConnectionSettings(pool).DisableDirectStreaming(); // maybe enable?

            _client = new ElasticClient(connection);
        }

        public async Task MigrateAsync(CancellationToken cancellationToken = default)
        {
            await CreateIndexAsync<DbDoujinshi>(cancellationToken);
            await CreateIndexAsync<DbBooruPost>(cancellationToken);
        }

        static readonly ConcurrentDictionary<Type, string> _indexNames = new ConcurrentDictionary<Type, string>();

        string GetIndexName<T>()
        {
            if (!_indexNames.TryGetValue(typeof(T), out var index))
            {
                var name = typeof(T).GetCustomAttribute<ElasticsearchTypeAttribute>()?.RelationName;

                _indexNames[typeof(T)] = index = $"{_options.ElasticIndexPrefix}-{name}".ToLowerInvariant();
            }

            return index;
        }

        async Task CreateIndexAsync<T>(CancellationToken cancellationToken = default) where T : class
        {
            var name = GetIndexName<T>();

            if ((await _client.Indices.ExistsAsync(name, ct: cancellationToken)).Exists)
                return;

            var response = await _client.Indices.CreateAsync(name, x => x.Map(m => m.AutoMap<T>()), cancellationToken);

            ValidateResponse(response);

            _logger.LogInformation("Created index '{0}'.", name);
        }

        public async Task<Doujinshi> GetDoujinshiAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var doc = await GetAsync<DbDoujinshi>(id.ToShortString(), cancellationToken);

            var doujinshi = new Doujinshi();
            doc.ApplyTo(doujinshi);

            return doujinshi;
        }

        public async Task<BooruPost> GetBooruPostAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var doc = await GetAsync<DbBooruPost>(id.ToShortString(), cancellationToken);

            var post = new BooruPost();
            doc.ApplyTo(post);

            return post;
        }

        async Task<TDocument> GetAsync<TDocument>(DocumentPath<TDocument> id, CancellationToken cancellationToken)
            where TDocument : class
        {
            var response = await _client.GetAsync(id, x => x.Index(GetIndexName<TDocument>()), cancellationToken);

            ValidateResponse(response);

            return response.Source;
        }

        public Task IndexAsync(Doujinshi doujinshi, CancellationToken cancellationToken = default)
        {
            var doc = new DbDoujinshi();
            doc.Apply(doujinshi);

            return IndexAsync(doc, cancellationToken);
        }

        public Task IndexAsync(BooruPost post, CancellationToken cancellationToken = default)
        {
            var doc = new DbBooruPost();
            doc.Apply(post);

            return IndexAsync(doc, cancellationToken);
        }

        async Task IndexAsync<TDocument>(TDocument doc, CancellationToken cancellationToken)
            where TDocument : class
        {
            var response = await _client.IndexAsync(doc, x => x.Index(GetIndexName<TDocument>()), cancellationToken);

            ValidateResponse(response);

            _logger.LogInformation($"Indexed {typeof(TDocument).Name}: {response.Id}");
        }

        public Task DeleteAsync(Doujinshi doujinshi, CancellationToken cancellationToken = default)
            => DeleteAsync<DbDoujinshi>(doujinshi.Id.ToShortString(), cancellationToken);

        public Task DeleteAsync(BooruPost post, CancellationToken cancellationToken = default)
            => DeleteAsync<DbBooruPost>(post.Id.ToShortString(), cancellationToken);

        async Task DeleteAsync<TDocument>(DocumentPath<TDocument> id, CancellationToken cancellationToken)
            where TDocument : class
        {
            var response = await _client.DeleteAsync(id, x => x.Index(GetIndexName<TDocument>()), cancellationToken);

            ValidateResponse(response);

            _logger.LogInformation($"Deleted {typeof(TDocument).Name}: {response.Id}");
        }

        static void ValidateResponse(IResponse response)
        {
            if (!response.IsValid)
                throw response.OriginalException;
        }
    }
}