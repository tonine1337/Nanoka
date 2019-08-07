using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nanoka.Core.Models;
using Newtonsoft.Json;

namespace Nanoka.Core.Client
{
    public abstract class DatabaseClientBase : IDatabaseClient
    {
        readonly CancellationTokenSource _backgroundTaskToken = new CancellationTokenSource();

        readonly JsonSerializer _serializer;
        readonly HttpClient _http;

        protected DatabaseClientBase(string endpoint,
                                     JsonSerializer serializer,
                                     HttpClient httpClient)
        {
            _serializer = serializer;
            _http       = httpClient;

            _http.BaseAddress = new Uri(endpoint);
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default) { }

        async Task<TResponse> Send<TResponse>(string path, HttpMethod method, CancellationToken cancellationToken)
        {
            var requestMessage = new HttpRequestMessage
            {
                Method     = method,
                RequestUri = new Uri(path, UriKind.Relative)
            };

            var responseMessage = await _http.SendAsync(requestMessage, cancellationToken);

            return await ConvertResponseAsync<TResponse>(responseMessage);
        }

        async Task<TResponse> Send<TRequest, TResponse>(string path,
                                                        HttpMethod method,
                                                        TRequest request,
                                                        CancellationToken cancellationToken)
        {
            string requestString;

            using (var writer = new StringWriter())
            {
                _serializer.Serialize(writer, request);
                requestString = writer.ToString();
            }

            var requestMessage = new HttpRequestMessage
            {
                Method     = method,
                RequestUri = new Uri(path, UriKind.Relative),
                Content    = new StringContent(requestString, Encoding.Default, "application/json")
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
            _backgroundTaskToken.Cancel();
            _backgroundTaskToken.Dispose();
        }
    }
}