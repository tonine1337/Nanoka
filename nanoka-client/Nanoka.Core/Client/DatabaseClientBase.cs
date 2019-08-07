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

        protected DatabaseClientBase(string endpoint, string secret, JsonSerializer serializer, HttpClient httpClient)
        {
            _serializer = serializer;
            _http       = httpClient;

            _userId     = secret.Substring(0, secret.Length / 2).ToGuid();
            _userSecret = secret.Substring(secret.Length / 2).ToGuid();

            _http.BaseAddress = new Uri(endpoint);
        }

        void LinkToken(ref CancellationToken token)
            => token = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, token).Token;

        DateTime _nextAuthTime;

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            LinkToken(ref cancellationToken);

            using (await _semaphore.EnterAsync(cancellationToken))
            {
                if (DateTime.Now < _nextAuthTime.AddSeconds(-5)) // reauthenticate slightly earlier
                    return;

                var response = await Send<AuthenticationResponse>(
                    "users/auth",
                    HttpMethod.Post,
                    new AuthenticationRequest
                    {
                        Id     = _userId,
                        Secret = _userSecret
                    },
                    cancellationToken);

                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", response.AccessToken);

                _nextAuthTime = response.Expiry;
            }
        }

        async Task<TResponse> Send<TResponse>(string path,
                                              HttpMethod method,
                                              CancellationToken cancellationToken)
        {
            LinkToken(ref cancellationToken);

            var requestMessage = new HttpRequestMessage
            {
                Method     = method,
                RequestUri = new Uri(path, UriKind.Relative)
            };

            var responseMessage = await _http.SendAsync(requestMessage, cancellationToken);

            return await ConvertResponseAsync<TResponse>(responseMessage);
        }

        async Task<TResponse> Send<TResponse>(string path,
                                              HttpMethod method,
                                              object request,
                                              CancellationToken cancellationToken)
        {
            LinkToken(ref cancellationToken);

            var requestMessage = new HttpRequestMessage
            {
                Method     = method,
                RequestUri = new Uri(path, UriKind.Relative),
                Content    = new StringContent(_serializer.Serialize(request), Encoding.Default, "application/json")
            };

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

        public Task<Doujinshi> GetDoujinshiAsync(Guid id, CancellationToken cancellationToken = default)
            => Send<Doujinshi>($"doujinshi/{id}", HttpMethod.Get, cancellationToken);

        public Task<BooruPost> GetBooruPostAsync(Guid id, CancellationToken cancellationToken = default)
            => Send<BooruPost>($"booru/{id}", HttpMethod.Get, cancellationToken);

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}