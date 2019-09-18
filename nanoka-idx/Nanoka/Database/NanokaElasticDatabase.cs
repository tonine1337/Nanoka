using System;
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

            _indexNames =
                typeof(NanokaElasticDatabase)
                   .Assembly
                   .GetTypes()
                   .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<ElasticsearchTypeAttribute>() != null)
                   .ToDictionary(t => t, t => (_options.IndexPrefix + t.GetCustomAttribute<ElasticsearchTypeAttribute>().RelationName).ToLowerInvariant());
        }

#region Migration

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

            await CreateIndexAsync<DbVote>(cancellationToken);
            await CreateIndexAsync<DbDeleteFile>(cancellationToken);
        }

        readonly Dictionary<Type, string> _indexNames;

        async Task<bool> CreateIndexAsync<T>(CancellationToken cancellationToken = default) where T : class
        {
            var name = _indexNames[typeof(T)];

            if ((await _client.Indices.ExistsAsync(name, ct: cancellationToken)).Exists)
            {
                _logger.LogInformation("Skipped creating index '{0}'.", name);
                return false;
            }

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

        public async Task ResetAsync(CancellationToken cancellationToken = default)
        {
            var response = await _client.Indices.DeleteAsync(Indices.All, ct: cancellationToken);

            ValidateResponse(response);
        }

#endregion

#region User

        public async Task<User> GetUserByIdAsync(string id, CancellationToken cancellationToken = default)
            => (await GetAsync<DbUser>(id, cancellationToken))?.ToUser();

        public async Task<User> GetUserByNameAsync(string username, CancellationToken cancellationToken = default)
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

        public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
            => user.Id = await IndexAsync(DbUser.FromUser(user), cancellationToken);

        public Task DeleteUserAsync(User user, CancellationToken cancellationToken = default)
            => DeleteAsync<DbUser>(user.Id, cancellationToken);

#endregion

#region Book

        public async Task<Book> GetBookAsync(string id, CancellationToken cancellationToken = default)
            => (await GetAsync<DbBook>(id, cancellationToken))?.ToBook();

        public async Task UpdateBookAsync(Book book, CancellationToken cancellationToken = default)
            => book.Id = await IndexAsync(DbBook.FromBook(book), cancellationToken);

        public Task DeleteBookAsync(Book book, CancellationToken cancellationToken = default)
            => DeleteAsync<DbBook>(book.Id, cancellationToken);

#endregion

#region Image

        public async Task<Image> GetImageAsync(string id, CancellationToken cancellationToken = default)
            => (await GetAsync<DbImage>(id, cancellationToken))?.ToImage();

        public async Task UpdateImageAsync(Image image, CancellationToken cancellationToken = default)
            => image.Id = await IndexAsync(DbImage.FromImage(image), cancellationToken);

        public Task DeleteImageAsync(Image image, CancellationToken cancellationToken = default)
            => DeleteAsync<DbImage>(image.Id, cancellationToken);

#endregion

#region Snapshot

        public async Task<Snapshot<T>> GetSnapshotAsync<T>(string id, string entityId, CancellationToken cancellationToken = default)
        {
            var snapshot = (await GetAsync<DbSnapshot>(id, cancellationToken))?.ToSnapshot<T>(_serializer);

            return snapshot?.EntityId == entityId
                ? snapshot
                : null;
        }

        public async Task<Snapshot<T>[]> GetSnapshotsAsync<T>(string entityId, CancellationToken cancellationToken = default)
        {
            var result = await SearchAsync<DbSnapshot>(
                (0, 256),
                q => q.Query(qq => qq.Bool(b => b.Filter(f => f.Term(t => t.Field(s => s.EntityType)
                                                                           .Value(Enum.Parse<NanokaEntity>(typeof(T).Name))),
                                                         f => f.Term(t => t.Field(s => s.EntityId)
                                                                           .Value(entityId)))))
                      .Sort(ss => ss.Descending(s => s.Time)),
                cancellationToken);

            return result.Items.ToArray(s => s.ToSnapshot<T>(_serializer));
        }

        public async Task UpdateSnapshotAsync<T>(Snapshot<T> snapshot, CancellationToken cancellationToken = default)
            => snapshot.Id = await IndexAsync(DbSnapshot.FromSnapshot(snapshot, _serializer), cancellationToken);

#endregion

#region Vote

        public async Task<Vote> GetVoteAsync(string userId, NanokaEntity entity, string entityId, CancellationToken cancellationToken = default)
            => (await GetAsync<DbVote>(DbVote.CreateId(userId, entity, entityId), cancellationToken))?.ToVote();

        public async Task UpdateVoteAsync(Vote vote, CancellationToken cancellationToken = default)
            => await IndexAsync(DbVote.FromVote(vote), cancellationToken);

        public async Task DeleteVoteAsync(Vote vote, CancellationToken cancellationToken = default)
            => await DeleteAsync<DbVote>(DbVote.CreateId(vote.UserId, vote.EntityType, vote.EntityId), cancellationToken);

        public async Task<int> DeleteVotesAsync(NanokaEntity entity, string entityId, CancellationToken cancellationToken = default)
        {
            var deleted = await DeleteAsync<DbVote>(
                q => q.Query(qq => qq.Bool(b => b.Filter(f => f.Term(t => t.Field(v => v.EntityType)
                                                                           .Value(entity)),
                                                         f => f.Term(t => t.Field(v => v.EntityId)
                                                                           .Value(entityId))))),
                cancellationToken);

            return deleted;
        }

#endregion

#region DeleteFile

        public async Task AddDeleteFilesAsync(string[] filenames, DateTime softDeleteTime, CancellationToken cancellationToken = default)
        {
            var files = filenames.ToArray(name => new DbDeleteFile
            {
                Id             = name,
                SoftDeleteTime = softDeleteTime
            });

            await IndexAsync(files, cancellationToken);
        }

        public async Task RemoveDeleteFileAsync(string[] filenames, CancellationToken cancellationToken = default)
            => await DeleteAsync<DbDeleteFile>(filenames, cancellationToken);

        public async Task<string[]> GetAndRemoveDeleteFilesAsync(DateTime maxSoftDeleteTime, CancellationToken cancellationToken = default)
        {
            var result = await SearchAsync<DbDeleteFile>(
                (0, 100),
                q => q.Query(qq => qq.Bool(b => b.Filter(ff => ff.DateRange(r => r.Field(f => f.SoftDeleteTime)
                                                                                  .LessThan(maxSoftDeleteTime)))))
                      .Sort(s => s.Ascending(f => f.SoftDeleteTime)),
                cancellationToken);

            if (result.Items.Count != 0)
                await DeleteAsync<DbDeleteFile>(result.Items.ToArray(x => x.Id), cancellationToken);

            return result.Items.ToArray(f => f.Id);
        }

#endregion

#region Client calls

        async Task<TDocument> GetAsync<TDocument>(DocumentPath<TDocument> id, CancellationToken cancellationToken)
            where TDocument : class
        {
            var response = await _client.GetAsync(
                id,
                x => x.Index(_indexNames[typeof(TDocument)]),
                cancellationToken);

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
                    x => query(x.Index(_indexNames[typeof(TDocument)])
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

        async Task<string> IndexAsync<TDocument>(TDocument doc, CancellationToken cancellationToken)
            where TDocument : class, IHasId
        {
            // id autogeneration
            if (string.IsNullOrEmpty(doc.Id))
                doc.Id = Snowflake.New;

            var response = await _client.IndexAsync(
                doc,
                x => x.Index(_indexNames[typeof(TDocument)])
                      .Refresh(_options.WaitIndexUpdate ? Refresh.WaitFor : Refresh.False),
                cancellationToken);

            ValidateResponse(response);

            _logger.LogInformation($"Indexed {typeof(TDocument).Name}: {doc.Id}");

            return doc.Id;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        async Task<string[]> IndexAsync<TDocument>(TDocument[] docs, CancellationToken cancellationToken)
            where TDocument : class, IHasId
        {
            if (docs.Length == 0)
                return new string[0];

            // id autogeneration
            foreach (var doc in docs)
            {
                if (string.IsNullOrEmpty(doc.Id))
                    doc.Id = Snowflake.New;
            }

            var response = await _client.BulkAsync(
                x => x.Index(_indexNames[typeof(TDocument)])
                      .IndexMany(docs)
                      .Refresh(_options.WaitIndexUpdate ? Refresh.WaitFor : Refresh.False),
                cancellationToken);

            ValidateResponse(response);

            _logger.LogInformation($"Indexed {response.Items.Count} {typeof(TDocument).Name}: {string.Join(", ", docs.Select(d => d.Id))}");

            return docs.ToArray(d => d.Id);
        }

        async Task DeleteAsync<TDocument>(string id, CancellationToken cancellationToken)
            where TDocument : class
        {
            var response = await _client.DeleteAsync<TDocument>(
                id,
                x => x.Index(_indexNames[typeof(TDocument)])
                      .Refresh(_options.WaitIndexUpdate ? Refresh.WaitFor : Refresh.False),
                cancellationToken);

            ValidateResponse(response);

            _logger.LogInformation($"Deleted {typeof(TDocument).Name}: {response.Id}");
        }

        async Task<int> DeleteAsync<TDocument>(Func<DeleteByQueryDescriptor<TDocument>, DeleteByQueryDescriptor<TDocument>> query,
                                               CancellationToken cancellationToken)
            where TDocument : class
        {
            var response = await _client.DeleteByQueryAsync<TDocument>(
                x => query(x.Index(_indexNames[typeof(TDocument)])
                            .Refresh(_options.WaitIndexUpdate)),
                cancellationToken);

            ValidateResponse(response);

            _logger.LogInformation($"Deleted {response.Deleted} {typeof(TDocument).Name} by query.");

            return (int) response.Deleted;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        async Task<int> DeleteAsync<TDocument>(string[] ids, CancellationToken cancellationToken)
            where TDocument : class
        {
            if (ids.Length == 0)
                return 0;

            var response = await _client.BulkAsync(
                x => x.Index(_indexNames[typeof(TDocument)])
                      .DeleteMany<TDocument>(ids)
                      .Refresh(_options.WaitIndexUpdate ? Refresh.WaitFor : Refresh.False),
                cancellationToken);

            ValidateResponse(response);

            _logger.LogInformation($"Deleted {response.Items.Count} {typeof(TDocument).Name}: {string.Join(", ", response.Items.Select(x => x.Id))}");

            return response.Items.Count;
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

#endregion

        public void Dispose() { }
    }
}