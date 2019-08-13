using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nanoka.Core.Models;
using Newtonsoft.Json;

namespace Nanoka.Core.Client
{
    public abstract class DatabaseClientBase : IDatabaseClient
    {
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        readonly JsonSerializer _serializer;
        readonly HttpClient _http;

        readonly Guid _userId;
        readonly Guid _userSecret;

        protected DatabaseClientBase(string endpoint,
                                     Guid userId,
                                     Guid userSecret,
                                     JsonSerializer serializer,
                                     HttpClient httpClient)
        {
            _serializer = serializer;
            _http       = httpClient;
            _userId     = userId;
            _userSecret = userSecret;

            _http.BaseAddress = new Uri(endpoint);
        }

        void LinkToken(ref CancellationToken token)
            => token = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, token).Token;

        DateTime _nextAuthTime = DateTime.UtcNow;

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            LinkToken(ref cancellationToken);

            using (await _semaphore.EnterAsync(cancellationToken))
            {
                if (DateTime.UtcNow < _nextAuthTime.AddSeconds(-5)) // reauthenticate slightly earlier
                    return;

                var response = await Send<AuthenticationResponse>(
                    "users/auth",
                    HttpMethod.Post,
                    new AuthenticationRequest
                    {
                        Id     = _userId,
                        Secret = _userSecret
                    },
                    cancellationToken,
                    false);

                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", response.AccessToken);

                _nextAuthTime = response.Expiry;
            }
        }

        async Task<TResponse> Send<TResponse>(string path,
                                              HttpMethod method,
                                              object request,
                                              CancellationToken cancellationToken,
                                              bool ensureAuth = true)
        {
            if (ensureAuth)
                await ConnectAsync(cancellationToken);

            LinkToken(ref cancellationToken);

            var requestMessage = new HttpRequestMessage
            {
                Method     = method,
                RequestUri = new Uri(path, UriKind.Relative)
            };

            if (request != null)
                requestMessage.Content = new StringContent(_serializer.Serialize(request),
                                                           Encoding.Default,
                                                           "application/json");

            var responseMessage = await _http.SendAsync(requestMessage, cancellationToken);

            return await ConvertResponseAsync<TResponse>(responseMessage);
        }

        sealed class ResponseWrapper<TBody>
        {
            [JsonProperty("error")]
            public bool Error { get; set; }

            [JsonProperty("status")]
            public int Status { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("body")]
            public TBody Body { get; set; }
        }

        async Task<TResponse> ConvertResponseAsync<TResponse>(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
                throw new DatabaseClientException((int) responseMessage.StatusCode, responseMessage.ReasonPhrase);

            ResponseWrapper<TResponse> response;

            using (var reader = new StringReader(await responseMessage.Content.ReadAsStringAsync()))
                response = _serializer.Deserialize<ResponseWrapper<TResponse>>(reader);

            if (response.Status == 404)
                return default;

            if (response.Error)
                throw new DatabaseClientException(response.Status, response.Message);

            return response.Body;
        }

        public Task<DatabaseInfo> GetDatabaseInfoAsync(CancellationToken cancellationToken = default)
            => Send<DatabaseInfo>("", HttpMethod.Get, null, cancellationToken);

        public Task<Doujinshi> GetDoujinshiAsync(Guid id, CancellationToken cancellationToken = default)
            => Send<Doujinshi>($"doujinshi/{id}", HttpMethod.Get, null, cancellationToken);

        public Task<SearchResult<Doujinshi>> SearchDoujinshiAsync(DoujinshiQuery query, CancellationToken cancellationToken = default)
            => Send<SearchResult<Doujinshi>>("doujinshi/search", HttpMethod.Post, query, cancellationToken);

        public Task<UploadState> CreateDoujinshiAsync(CreateDoujinshiRequest request, CancellationToken cancellationToken = default)
            => Send<UploadState>("doujinshi", HttpMethod.Post, request, cancellationToken);

        public Task<Doujinshi> UpdateDoujinshiAsync(Guid id, DoujinshiBase doujinshi, CancellationToken cancellationToken = default)
            => Send<Doujinshi>($"doujinshi/{id}", HttpMethod.Put, doujinshi, cancellationToken);

        public Task DeleteDoujinshiAsync(Guid id, string reason, CancellationToken cancellationToken = default)
            => Send<object>($"doujinshi/{id}?reason={reason}", HttpMethod.Delete, null, cancellationToken);

        public Task<UploadState> CreateDoujinshiVariantAsync(Guid id, CreateDoujinshiVariantRequest request, CancellationToken cancellationToken = default)
            => Send<UploadState>($"doujinshi/{id}/variants", HttpMethod.Post, request, cancellationToken);

        public Task<DoujinshiVariant> UpdateDoujinshiVariantAsync(Guid id, Guid variantId, DoujinshiVariantBase variant, CancellationToken cancellationToken = default)
            => Send<DoujinshiVariant>($"doujinshi/{id}/variants/{variantId}", HttpMethod.Put, variant, cancellationToken);

        public Task DeleteDoujinshiVariantAsync(Guid id, Guid variantId, string reason, CancellationToken cancellationToken = default)
            => Send<object>($"doujinshi/{id}/variants/{variantId}?reason={reason}", HttpMethod.Delete, null, cancellationToken);

        public Task<BooruPost> GetBooruPostAsync(Guid id, CancellationToken cancellationToken = default)
            => Send<BooruPost>($"booru/{id}", HttpMethod.Get, null, cancellationToken);

        public Task<UploadState> GetUploadStateAsync(Guid id, CancellationToken cancellationToken = default)
            => Send<UploadState>($"uploads/{id}", HttpMethod.Get, null, cancellationToken);

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}