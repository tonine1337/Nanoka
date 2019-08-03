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

#region Doujinshi

        public async Task<Doujinshi> GetDoujinshiAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var doc = await GetAsync<DbDoujinshi>(id.ToShortString(), cancellationToken);

            return doc.ApplyTo(new Doujinshi());
        }

        public Task IndexAsync(Doujinshi doujinshi, CancellationToken cancellationToken = default)
            => IndexAsync(new DbDoujinshi().Apply(doujinshi), cancellationToken);

        public Task DeleteAsync(Doujinshi doujinshi, CancellationToken cancellationToken = default)
            => DeleteAsync<DbDoujinshi>(doujinshi.Id.ToShortString(), cancellationToken);

#endregion

#region Booru

        public async Task<BooruPost> GetBooruPostAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var doc = await GetAsync<DbBooruPost>(id.ToShortString(), cancellationToken);

            return doc.ApplyTo(new BooruPost());
        }

        public Task IndexAsync(BooruPost post, CancellationToken cancellationToken = default)
            => IndexAsync(new DbBooruPost().Apply(post), cancellationToken);

        public Task DeleteAsync(BooruPost post, CancellationToken cancellationToken = default)
            => DeleteAsync<DbBooruPost>(post.Id.ToShortString(), cancellationToken);

#endregion

#region User

        public async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var doc = await GetAsync<DbUser>(id.ToShortString(), cancellationToken);

            return doc.ApplyTo(new User());
        }

        public Task IndexAsync(User user, CancellationToken cancellationToken = default)
            => IndexAsync(new DbUser().Apply(user), cancellationToken);

        public Task DeleteAsync(User user, CancellationToken cancellationToken = default)
            => DeleteAsync<DbUser>(user.Id.ToShortString(), cancellationToken);

#endregion

        async Task<TDocument> GetAsync<TDocument>(DocumentPath<TDocument> id, CancellationToken cancellationToken)
            where TDocument : class
        {
            var response = await _client.GetAsync(id, x => x.Index(GetIndexName<TDocument>()), cancellationToken);

            ValidateResponse(response);

            return response.Source;
        }

        async Task IndexAsync<TDocument>(TDocument doc, CancellationToken cancellationToken)
            where TDocument : class
        {
            var response = await _client.IndexAsync(doc, x => x.Index(GetIndexName<TDocument>()), cancellationToken);

            ValidateResponse(response);

            _logger.LogInformation($"Indexed {typeof(TDocument).Name}: {response.Id}");
        }

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