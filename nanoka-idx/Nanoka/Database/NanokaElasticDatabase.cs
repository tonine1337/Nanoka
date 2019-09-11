using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nanoka.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Database
{
    public class NanokaElasticDatabase : INanokaDatabase
    {
        readonly ElasticClient _client;

        readonly ElasticOptions _options;
        readonly ILogger<NanokaElasticDatabase> _logger;
        readonly JsonSerializer _serializer;
        readonly PasswordHashHelper _hash;

        public NanokaElasticDatabase(IOptions<ElasticOptions> options, ILogger<NanokaElasticDatabase> logger, JsonSerializer serializer, PasswordHashHelper hash)
        {
            _options    = options.Value;
            _logger     = logger;
            _serializer = serializer;
            _hash       = hash;

            if (_options.Endpoint == null)
                throw new NanokaDatabaseException("Elasticsearch endpoint is not configured.");

            var pool       = new SingleNodeConnectionPool(new Uri(_options.Endpoint));
            var connection = new ConnectionSettings(pool).DisableDirectStreaming(); // maybe enable?

            _client = new ElasticClient(connection);
        }

        const string _defaultAdminUsername = "admin";
        const string _defaultAdminPassword = "admin";

        public async Task MigrateAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Migrating indexes...");

            await CreateIndexAsync<DbBook>(cancellationToken);
            await CreateIndexAsync<DbImage>(cancellationToken);
            await CreateIndexAsync<DbSnapshot>(cancellationToken);

            if (await CreateIndexAsync<DbUser>(cancellationToken))
            {
                // create admin user
                var user = new User
                {
                    Username    = _defaultAdminUsername,
                    Secret      = _hash.Hash(_defaultAdminPassword),
                    Permissions = UserPermissions.Administrator
                };

                await UpdateUserAsync(user, cancellationToken);

                _logger.LogWarning($"Administrator user created. USERNAME:{_defaultAdminUsername} --- PASSWORD:{_defaultAdminPassword}");
            }
        }

        static readonly ConcurrentDictionary<Type, string> _indexNames = new ConcurrentDictionary<Type, string>();

        string IndexName<T>()
        {
            if (!_indexNames.TryGetValue(typeof(T), out var index))
            {
                var name = typeof(T).GetCustomAttribute<ElasticsearchTypeAttribute>()?.RelationName;

                _indexNames[typeof(T)] = index = (_options.IndexPrefix + name).ToLowerInvariant();
            }

            return index;
        }

        async Task<bool> CreateIndexAsync<T>(CancellationToken cancellationToken = default) where T : class
        {
            var name = IndexName<T>();

            if ((await _client.Indices.ExistsAsync(name, ct: cancellationToken)).Exists)
                return false;

            var response = await _client.Indices.CreateAsync(
                name,
                x => x.Map(m => m.AutoMap<T>())
                      .Settings(s => s.NumberOfShards(_options.ShardCount)
                                      .NumberOfReplicas(_options.ReplicaCount)),
                cancellationToken);

            ValidateResponse(response);

            _logger.LogInformation("Created index '{0}'.", name);

            return true;
        }

        void ValidateResponse(IResponse response)
        {
            if (!response.IsValid)
            {
                _logger.LogDebug(response.DebugInformation);

                if (response.OriginalException != null)
                    throw response.OriginalException;
            }
        }

        public async Task<User> GetUserAsync(int id, CancellationToken cancellationToken = default)
            => (await GetAsync<DbUser>(id, cancellationToken)).ToUser();

        public async Task<User> GetUserAsync(string username, CancellationToken cancellationToken = default)
        {
            var result = await SearchAsync<DbUser>(
                (0, 1),
                q => q.Query(qq => qq.Term(t => t.Field(u => u.Username)
                                                 .Value(username))),
                cancellationToken);

            if (result.Total > 1)
                _logger.LogWarning($"{result.Total} users with an identical username exist: {username}");

            return result.Items.FirstOrDefault()?.ToUser();
        }

        public Task<int> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
            => IndexAsync(DbUser.FromUser(user), cancellationToken);

        public Task DeleteUserAsync(int id, CancellationToken cancellationToken = default)
            => DeleteAsync<DbUser>(id, cancellationToken);

        public async Task<Book> GetBookAsync(int id, CancellationToken cancellationToken = default)
            => (await GetAsync<DbBook>(id, cancellationToken)).ToBook();

        public Task<int> UpdateBookAsync(Book book, CancellationToken cancellationToken = default)
            => IndexAsync(DbBook.FromBook(book), cancellationToken);

        public Task DeleteBookAsync(int id, CancellationToken cancellationToken = default)
            => DeleteAsync<DbBook>(id, cancellationToken);

        public async Task<Image> GetImageAsync(int id, CancellationToken cancellationToken = default)
            => (await GetAsync<DbImage>(id, cancellationToken)).ToImage();

        public Task<int> UpdateImageAsync(Image image, CancellationToken cancellationToken = default)
            => IndexAsync(DbImage.FromImage(image), cancellationToken);

        public Task DeleteImageAsync(int id, CancellationToken cancellationToken = default)
            => DeleteAsync<DbImage>(id, cancellationToken);

        public Task<int> AddSnapshotAsync<T>(Snapshot<T> snapshot, CancellationToken cancellationToken = default)
            => IndexAsync(DbSnapshot.FromSnapshot(snapshot, _serializer), cancellationToken);

        public async Task<Snapshot<T>> GetSnapshotAsync<T>(int id, CancellationToken cancellationToken = default)
            => (await GetAsync<DbSnapshot>(id, cancellationToken)).ToSnapshot<T>(_serializer);

        static SnapshotEntity ConvertSnapshotEntity<T>()
        {
            var name = typeof(T).Name;

            if (Enum.TryParse<SnapshotEntity>(name, out var entity))
                return entity;

            throw new NotSupportedException($"Cannot retrieve snapshots of type '{typeof(T).FullName}'.");
        }

        public async Task<Snapshot<T>[]> GetSnapshotsAsync<T>(int entityId, CancellationToken cancellationToken = default)
        {
            var result = await SearchAsync<DbSnapshot>(
                (0, 256),
                q => q.Query(qq => qq.Bool(b => b.Filter(f => f.Term(t => t.Field(s => s.Entity)
                                                                           .Value(ConvertSnapshotEntity<T>())),
                                                         f => f.Term(t => t.Field(s => s.EntityId)
                                                                           .Value(entityId)))))
                      .Sort(ss => ss.Descending(s => s.Time)),
                cancellationToken);

            return result.Items.ToArray(s => s.ToSnapshot<T>(_serializer));
        }

        async Task<TDocument> GetAsync<TDocument>(DocumentPath<TDocument> id, CancellationToken cancellationToken)
            where TDocument : class
        {
            var response = await _client.GetAsync(id, x => x.Index(IndexName<TDocument>()), cancellationToken);

            ValidateResponse(response);

            return response.Source;
        }

        async Task<SearchResult<TDocument>> SearchAsync<TDocument>(Range<int> range,
                                                                   Func<SearchDescriptor<TDocument>, SearchDescriptor<TDocument>> query,
                                                                   CancellationToken cancellationToken)
            where TDocument : class
        {
            using (var measure = new MeasureContext())
            {
                var response = await _client.SearchAsync<TDocument>(
                    x => query(x.Index(IndexName<TDocument>())
                                .Skip(range.Min)
                                .Take(range.Max - range.Min)),
                    cancellationToken);

                ValidateResponse(response);

                return new SearchResult<TDocument>
                {
                    Took         = response.Took,
                    TookAccurate = measure.Milliseconds,
                    Total        = response.Total,
                    Items        = response.Documents
                };
            }
        }

        sealed class SearchResult<TDocument>
        {
            public long Took;
            public double TookAccurate;
            public long Total;
            public IReadOnlyCollection<TDocument> Items;
        }

        async Task<int> IndexAsync<TDocument>(TDocument doc, CancellationToken cancellationToken)
            where TDocument : class
        {
            var response = await _client.IndexAsync(doc, x => x.Index(IndexName<TDocument>()), cancellationToken);

            ValidateResponse(response);

            _logger.LogInformation($"Indexed {typeof(TDocument).Name}: {response.Id}");

            // assuming we always use integer ID
            return int.Parse(response.Id);
        }

        async Task DeleteAsync<TDocument>(DocumentPath<TDocument> id, CancellationToken cancellationToken)
            where TDocument : class
        {
            var response = await _client.DeleteAsync(id, x => x.Index(IndexName<TDocument>()), cancellationToken);

            ValidateResponse(response);

            _logger.LogInformation($"Deleted {typeof(TDocument).Name}: {response.Id}");
        }

        public void Dispose() { }
    }
}