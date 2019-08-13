using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nanoka.Core;
using Nanoka.Core.Models;
using Nest;
using Newtonsoft.Json;

namespace Nanoka.Web.Database
{
    public class NanokaDatabase
    {
        readonly ElasticClient _client;

        readonly NanokaOptions _options;
        readonly ILogger<NanokaDatabase> _logger;
        readonly JsonSerializer _serializer;

        public NanokaDatabase(IOptions<NanokaOptions> options,
                              ILogger<NanokaDatabase> logger,
                              JsonSerializer serializer)
        {
            _options    = options.Value;
            _logger     = logger;
            _serializer = serializer;

            if (_options.ElasticEndpoint == null)
                throw new NanokaDatabaseException("Elasticsearch endpoint is not configured.");

            var pool       = new SingleNodeConnectionPool(new Uri(_options.ElasticEndpoint));
            var connection = new ConnectionSettings(pool).DisableDirectStreaming(); // maybe enable?

            _client = new ElasticClient(connection);
        }

        public async Task MigrateAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Migrating indexes...");

            await CreateIndexAsync<DbDoujinshi>(cancellationToken);
            await CreateIndexAsync<DbBooruPost>(cancellationToken);
            await CreateIndexAsync<DbSnapshot>(cancellationToken);

            if (await CreateIndexAsync<DbUser>(cancellationToken))
            {
                // create admin user
                var user = new User
                {
                    Id          = Guid.NewGuid(),
                    Secret      = Extensions.SecureGuid(),
                    Username    = "admin",
                    Registered  = DateTime.UtcNow,
                    Permissions = UserPermissions.Administrator
                };

                await IndexAsync(user, cancellationToken);

                _logger.LogWarning($"Administrator user created: ID:{user.Id} --- SECRET:{user.Secret}");
            }
        }

        static readonly ConcurrentDictionary<Type, string> _indexNames = new ConcurrentDictionary<Type, string>();

        string GetIndexName<T>()
        {
            if (!_indexNames.TryGetValue(typeof(T), out var index))
            {
                var name = typeof(T).GetCustomAttribute<ElasticsearchTypeAttribute>()?.RelationName;

                _indexNames[typeof(T)] = index = (_options.ElasticIndexPrefix + name).ToLowerInvariant();
            }

            return index;
        }

        async Task<bool> CreateIndexAsync<T>(CancellationToken cancellationToken = default) where T : class
        {
            var name = GetIndexName<T>();

            if ((await _client.Indices.ExistsAsync(name, ct: cancellationToken)).Exists)
                return false;

            var response = await _client.Indices.CreateAsync(
                name,
                x => x.Map(m => m.AutoMap<T>())
                      .Settings(s => s.NumberOfShards(_options.ElasticShardCount)
                                      .NumberOfReplicas(_options.ElasticReplicaCount)),
                cancellationToken);

            ValidateResponse(response);

            _logger.LogInformation("Created index '{0}'.", name);

            return true;
        }

#region Doujinshi

        public async Task<Doujinshi> GetDoujinshiAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var doc = await GetAsync<DbDoujinshi>(id.ToShortString(), cancellationToken);

            return doc?.ApplyTo(new Doujinshi());
        }

        public Task IndexAsync(Doujinshi doujinshi, CancellationToken cancellationToken = default)
            => IndexAsync(new DbDoujinshi().Apply(doujinshi), cancellationToken);

        public Task IndexSnapshotAsync(Snapshot<Doujinshi> snapshot, CancellationToken cancellationToken = default)
            => IndexAsync(new DbSnapshot().Apply(snapshot, _serializer), cancellationToken);

        public Task DeleteAsync(Doujinshi doujinshi, CancellationToken cancellationToken = default)
            => DeleteAsync<DbDoujinshi>(doujinshi.Id.ToShortString(), cancellationToken);

        public async Task<SearchResult<Doujinshi>> SearchAsync(DoujinshiQuery query,
                                                               CancellationToken cancellationToken = default)
        {
            var measure = new MeasureContext();

            var response = await _client.SearchAsync<DbDoujinshi>(
                x => x.Index(GetIndexName<DbDoujinshi>())
                      .Skip(query.Offset)
                      .Take(query.Limit)
                      .MultiQuery(
                           q => q.Text(query.All)
                                 .Range(query.UploadTime, d => d.UploadTime)
                                 .Range(query.UpdateTime, d => d.UpdateTime)
                                 .Text(query.OriginalName, d => d.OriginalName)
                                 .Text(query.RomanizedName, d => d.RomanizedName)
                                 .Text(query.EnglishName, d => d.EnglishName)
                                 .Filter(query.Category, d => d.Category)
                                 .Range(query.Score, d => d.Score)
                                 .Range(query.PageCount, d => d.PageCounts))
                      .NestedMultiQuery(
                           d => d.Variants,
                           q =>
                           {
                               q.Text(query.All);

                               if (query.Metas != null)
                                   foreach (var (meta, metaQuery) in query.Metas)
                                   {
                                       switch (meta)
                                       {
                                           case DoujinshiMeta.Artist:
                                               q.Text(metaQuery, d => d.Variants.First().Artist);
                                               break;
                                           case DoujinshiMeta.Group:
                                               q.Text(metaQuery, d => d.Variants.First().Group);
                                               break;
                                           case DoujinshiMeta.Parody:
                                               q.Text(metaQuery, d => d.Variants.First().Parody);
                                               break;
                                           case DoujinshiMeta.Character:
                                               q.Text(metaQuery, d => d.Variants.First().Character);
                                               break;
                                           case DoujinshiMeta.Language:
                                               q.Text(metaQuery, d => d.Variants.First().Language);
                                               break;
                                           case DoujinshiMeta.Tag:
                                               q.Text(metaQuery, d => d.Variants.First().Tag);
                                               break;
                                           case DoujinshiMeta.Convention:
                                               q.Text(metaQuery, d => d.Variants.First().Convention);
                                               break;
                                       }
                                   }

                               q.Text(query.Source, d => d.Variants.First().Source);

                               return q;
                           })
                      .MultiSort(query.Sorting,
                                 sort =>
                                 {
                                     switch (sort)
                                     {
                                         case DoujinshiQuerySort.UploadTime:    return d => d.UploadTime;
                                         case DoujinshiQuerySort.UpdateTime:    return d => d.UpdateTime;
                                         case DoujinshiQuerySort.OriginalName:  return d => d.OriginalName.Suffix("keyword");
                                         case DoujinshiQuerySort.RomanizedName: return d => d.RomanizedName.Suffix("keyword");
                                         case DoujinshiQuerySort.EnglishName:   return d => d.EnglishName.Suffix("keyword");
                                         case DoujinshiQuerySort.Score:         return d => d.Score;
                                         case DoujinshiQuerySort.PageCount:     return d => d.PageCounts;

                                         default: throw new NotSupportedException();
                                     }
                                 }),
                cancellationToken);

            ValidateResponse(response);

            return ConvertSearchResponse(response, d => d.ApplyTo(new Doujinshi()), measure);
        }

#endregion

#region Booru

        public async Task<BooruPost> GetBooruPostAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var doc = await GetAsync<DbBooruPost>(id.ToShortString(), cancellationToken);

            return doc?.ApplyTo(new BooruPost());
        }

        public Task IndexAsync(BooruPost post, CancellationToken cancellationToken = default)
            => IndexAsync(new DbBooruPost().Apply(post), cancellationToken);

        public Task IndexSnapshotAsync(Snapshot<BooruPost> snapshot, CancellationToken cancellationToken = default)
            => IndexAsync(new DbSnapshot().Apply(snapshot, _serializer), cancellationToken);

        public Task DeleteAsync(BooruPost post, CancellationToken cancellationToken = default)
            => DeleteAsync<DbBooruPost>(post.Id.ToShortString(), cancellationToken);

        public async Task<SearchResult<BooruPost>> SearchAsync(BooruQuery query,
                                                               CancellationToken cancellationToken = default)
        {
            var measure = new MeasureContext();

            var response = await _client.SearchAsync<DbBooruPost>(
                x => x.Index(GetIndexName<DbBooruPost>())
                      .Skip(query.Offset)
                      .Take(query.Limit)
                      .MultiQuery(
                           q =>
                           {
                               q.Text(query.All)
                                .Range(query.UploadTime, p => p.UploadTime)
                                .Range(query.UpdateTime, p => p.UpdateTime);

                               if (query.Tags != null)
                                   foreach (var (tag, tagQuery) in query.Tags)
                                   {
                                       switch (tag)
                                       {
                                           case BooruTag.Artist:
                                               q.Text(tagQuery, p => p.Artist);
                                               break;
                                           case BooruTag.Character:
                                               q.Text(tagQuery, p => p.Character);
                                               break;
                                           case BooruTag.Copyright:
                                               q.Text(tagQuery, p => p.Copyright);
                                               break;
                                           case BooruTag.Metadata:
                                               q.Text(tagQuery, p => p.Metadata);
                                               break;
                                           case BooruTag.General:
                                               q.Text(tagQuery, p => p.General);
                                               break;
                                       }
                                   }

                               q.Filter(query.Rating, p => p.Rating)
                                .Range(query.Score, p => p.Score)
                                .Text(query.Source, p => p.Source)
                                .Range(query.Width, p => p.Width)
                                .Range(query.Height, p => p.Height)
                                .Text(query.MediaType, p => p.MediaType);

                               return q;
                           })
                      .MultiSort(
                           query.Sorting,
                           sort =>
                           {
                               switch (sort)
                               {
                                   case BooruQuerySort.UploadTime: return p => p.UploadTime;
                                   case BooruQuerySort.UpdateTime: return p => p.UpdateTime;
                                   case BooruQuerySort.Rating:     return p => p.Rating;
                                   case BooruQuerySort.Score:      return p => p.Score;
                                   case BooruQuerySort.Width:      return p => p.Width;
                                   case BooruQuerySort.Height:     return p => p.Height;

                                   default: throw new NotSupportedException();
                               }
                           }),
                cancellationToken);

            ValidateResponse(response);

            return ConvertSearchResponse(response, p => p.ApplyTo(new BooruPost()), measure);
        }

#endregion

#region User

        public async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var doc = await GetAsync<DbUser>(id.ToShortString(), cancellationToken);

            return doc?.ApplyTo(new User());
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

        static SearchResult<TResult> ConvertSearchResponse<T, TResult>(ISearchResponse<T> response,
                                                                       Func<T, TResult> convert,
                                                                       MeasureContext measure = null)
            where T : class
            => new SearchResult<TResult>
            {
                Total = (int) response.Total,
                Items = response.Documents.Select(convert).ToArray(),
                Took  = measure?.Milliseconds ?? response.Took
            };

        void ValidateResponse(IResponse response)
        {
            if (!response.IsValid)
            {
                _logger.LogDebug(response.DebugInformation);

                if (response.OriginalException != null)
                    throw response.OriginalException;
            }
        }
    }
}