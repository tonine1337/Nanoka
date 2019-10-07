using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

            // migrate indexes parallel
            await Task.WhenAll(
                CreateIndexAsync<DbBook>(cancellationToken),
                CreateIndexAsync<DbImage>(cancellationToken),
                CreateIndexAsync<DbSnapshot>(cancellationToken),
                CreateIndexAsync<DbVote>(cancellationToken),
                CreateIndexAsync<DbDeleteFile>(cancellationToken),
                createUserIndexAsync());

            async Task createUserIndexAsync()
            {
                if (await CreateIndexAsync<DbUser>(cancellationToken))
                {
                    // create admin user
                    var user = new User
                    {
                        Username    = _defaultAdminUsername,
                        Secret      = _hash.Hash(_defaultAdminPassword),
                        Permissions = UserPermissions.Administrator
                    };

                    await ((IUserRepository) this).UpdateAsync(user, cancellationToken);

                    _logger.LogWarning($"Administrator user created. USERNAME:{_defaultAdminUsername} --- PASSWORD:{_defaultAdminPassword}");
                }
            }
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

        async Task<User> IUserRepository.GetByIdAsync(string id, CancellationToken cancellationToken)
            => (await GetAsync<DbUser>(id, cancellationToken))?.ToUser();

        async Task<User> IUserRepository.GetByNameAsync(string username, CancellationToken cancellationToken)
        {
            var result = await SearchAsync<DbUser, User>(
                (0, 1),
                q => q.Query(qq => qq.Term(t => t.Field(u => u.Username)
                                                 .Value(username))),
                u => u.ToUser(),
                cancellationToken);

            if (result.Total > 1)
                _logger.LogWarning($"{result.Total} users with an identical username exist: {username}");

            return result.Items.FirstOrDefault();
        }

        async Task IUserRepository.UpdateAsync(User user, CancellationToken cancellationToken)
            => user.Id = await IndexAsync(DbUser.FromUser(user), cancellationToken);

        Task IUserRepository.DeleteAsync(User user, CancellationToken cancellationToken)
            => DeleteAsync<DbUser>(user.Id, cancellationToken);

#endregion

#region Book

        async Task<Book> IBookRepository.GetAsync(string id, CancellationToken cancellationToken)
            => (await GetAsync<DbBook>(id, cancellationToken))?.ToBook();

        async Task IBookRepository.UpdateAsync(Book book, CancellationToken cancellationToken)
            => book.Id = await IndexAsync(DbBook.FromBook(book), cancellationToken);

        Task IBookRepository.DeleteAsync(Book book, CancellationToken cancellationToken)
            => DeleteAsync<DbBook>(book.Id, cancellationToken);

        async Task<SearchResult<Book>> IBookRepository.SearchAsync(BookQuery query, CancellationToken cancellationToken)
        {
            var results = await SearchAsync<DbBook, Book>(
                (query.Offset, query.Limit),
                _ => _.MultiQuery(q =>
                       {
                           q.Text(query.Name, b => b.Name)
                            .Range(query.Score, b => b.Score);

                           if (query.Tags != null)
                               foreach (var (k, v) in query.Tags)
                                   q.Text(v, GetBookTagsPath(k));

                           q.Filter(query.Category, b => b.Category)
                            .Filter(query.Rating, b => b.Rating)
                            .Range(query.PageCount, b => b.PageCounts)
                            .Filter(query.Language, b => b.Languages)
                            .Filter(query.IsColor, b => b.IsColor)
                            .Filter(query.Source.Project(s => s.ToString()), b => b.Sources);

                           return q;
                       })
                      .MultiSort(query.Sorting, GetBookSortPath),
                b => b.ToBook(),
                cancellationToken);

            return results;
        }

        static Expression<Func<DbBook, object>> GetBookTagsPath(BookTag tag)
        {
            switch (tag)
            {
                default:                 return b => b.TagsGeneral;
                case BookTag.Artist:     return b => b.TagsArtist;
                case BookTag.Parody:     return b => b.TagsParody;
                case BookTag.Character:  return b => b.TagsCharacter;
                case BookTag.Convention: return b => b.TagsConvention;
                case BookTag.Series:     return b => b.TagsSeries;
            }
        }

        static Expression<Func<DbBook, object>> GetBookSortPath(BookSort sort)
        {
            switch (sort)
            {
                default:                 return null;
                case BookSort.Score:     return b => b.Score;
                case BookSort.PageCount: return b => b.PageCounts;
            }
        }

#endregion

#region Image

        async Task<Image> IImageRepository.GetAsync(string id, CancellationToken cancellationToken)
            => (await GetAsync<DbImage>(id, cancellationToken))?.ToImage();

        async Task IImageRepository.UpdateAsync(Image image, CancellationToken cancellationToken)
            => image.Id = await IndexAsync(DbImage.FromImage(image), cancellationToken);

        Task IImageRepository.DeleteAsync(Image image, CancellationToken cancellationToken)
            => DeleteAsync<DbImage>(image.Id, cancellationToken);

#endregion

#region Snapshot

        async Task<Snapshot<T>> ISnapshotRepository.GetAsync<T>(string id, CancellationToken cancellationToken)
            => (await GetAsync<DbSnapshot>(id, cancellationToken))?.ToSnapshot<T>(_serializer);

        async Task<Snapshot<T>[]> ISnapshotRepository.GetAsync<T>(string entityId, int start, int count, bool chronological, CancellationToken cancellationToken)
        {
            var result = await SearchAsync<DbSnapshot, Snapshot<T>>(
                (start, count),
                q => q.Query(qq => qq.Bool(b => b.Filter(f => f.Term(t => t.Field(s => s.EntityType)
                                                                           .Value(Enum.Parse<NanokaEntity>(typeof(T).Name))),
                                                         f => f.Term(t => t.Field(s => s.EntityId)
                                                                           .Value(entityId)))))
                      .Sort(ss => chronological
                                ? ss.Ascending(s => s.Time)
                                : ss.Descending(s => s.Time)),
                s => s.ToSnapshot<T>(_serializer),
                cancellationToken);

            return result.Items;
        }

        async Task ISnapshotRepository.UpdateAsync<T>(Snapshot<T> snapshot, CancellationToken cancellationToken)
            => snapshot.Id = await IndexAsync(DbSnapshot.FromSnapshot(snapshot, _serializer), cancellationToken);

#endregion

#region Vote

        async Task<Vote> IVoteRepository.GetAsync(string userId, NanokaEntity entity, string entityId, CancellationToken cancellationToken)
            => (await GetAsync<DbVote>(DbVote.CreateId(userId, entity, entityId), cancellationToken))?.ToVote();

        async Task IVoteRepository.UpdateAsync(Vote vote, CancellationToken cancellationToken)
            => await IndexAsync(DbVote.FromVote(vote), cancellationToken);

        async Task IVoteRepository.DeleteAsync(Vote vote, CancellationToken cancellationToken)
            => await DeleteAsync<DbVote>(DbVote.CreateId(vote.UserId, vote.EntityType, vote.EntityId), cancellationToken);

        async Task<int> IVoteRepository.DeleteAsync(NanokaEntity entity, string entityId, CancellationToken cancellationToken)
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

        async Task IDeleteFileRepository.AddAsync(string[] filenames, DateTime softDeleteTime, CancellationToken cancellationToken)
        {
            var files = filenames.ToArray(name => new DbDeleteFile
            {
                Id             = name,
                SoftDeleteTime = softDeleteTime
            });

            await IndexAsync(files, cancellationToken);
        }

        async Task IDeleteFileRepository.RemoveAsync(string[] filenames, CancellationToken cancellationToken)
            => await DeleteAsync<DbDeleteFile>(filenames, cancellationToken);

        async Task<string[]> IDeleteFileRepository.GetAndRemoveAsync(DateTime maxSoftDeleteTime, CancellationToken cancellationToken)
        {
            var result = await SearchAsync<DbDeleteFile, string>(
                (0, 100),
                q => q.Query(qq => qq.Bool(b => b.Filter(ff => ff.DateRange(r => r.Field(f => f.SoftDeleteTime)
                                                                                  .LessThan(maxSoftDeleteTime)))))
                      .Sort(s => s.Ascending(f => f.SoftDeleteTime)),
                f => f.Id,
                cancellationToken);

            if (result.Items.Length != 0)
                await DeleteAsync<DbDeleteFile>(result.Items, cancellationToken);

            return result.Items;
        }

#endregion

#region Internal

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

        async Task<SearchResult<T>> SearchAsync<TDocument, T>((int start, int count) range,
                                                              Func<SearchDescriptor<TDocument>, SearchDescriptor<TDocument>> query,
                                                              Func<TDocument, T> project,
                                                              CancellationToken cancellationToken)
            where TDocument : class
        {
            // count=0 optimization
            if (range.count <= 0)
                return new SearchResult<T> { Items = new T[0] };

            using (var measure = new MeasureContext())
            {
                var response = await _client.SearchAsync<TDocument>(
                    x => query(x.Index(_indexNames[typeof(TDocument)])
                                .Skip(range.start)
                                .Take(range.count)),
                    cancellationToken);

                ValidateResponse(response);

                return new SearchResult<T>
                {
                    Took         = response.Took,
                    TookAccurate = measure.Milliseconds,
                    Total        = response.Total,
                    Items        = response.Documents.ToArray(project)
                };
            }
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