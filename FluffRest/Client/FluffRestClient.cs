using FluffRest.Exception;
using FluffRest.Request;
using FluffRest.Settings;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FluffRest.Client
{
    public class FluffRestClient : IFluffRestClient
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        private readonly FluffClientSettings _settings;

        /// <summary>
        /// Create a new rest client, using an existing HttpClient. It can be configured via the <see cref="FluffClientSettings"/> object.
        /// </summary>
        /// <param name="baseUrl">Root url of your api, you will specify endpoints later.</param>
        /// <param name="httpClient">HttpClient from your app</param>
        /// <param name="settings">Optional settings for deep configuration see <see cref="FluffClientSettings"/> object</param>
        /// <exception cref="System.ArgumentException">If the base url is null or empty or if the http client is null</exception>
        public FluffRestClient(string baseUrl, HttpClient httpClient, FluffClientSettings settings = null)
        {
            _baseUrl = !string.IsNullOrEmpty(baseUrl) ? baseUrl : throw new System.ArgumentException($"{nameof(baseUrl)} is null or empty, please provide a base url for api calls");
            _httpClient = httpClient ?? throw new System.ArgumentException($"{nameof(httpClient)} is null, please provide a http client");
            _settings = settings ?? new FluffClientSettings();
        }

        public string BaseUrl => _baseUrl;

        public FluffClientSettings Settings => _settings;

        public IFluffRequest Get(string route)
        {
            return Request(HttpMethod.Get, route);
        }

        public IFluffRequest Post(string route)
        {
            return Request(HttpMethod.Post, route);
        }

        public IFluffRequest Patch(string route)
        {
            return Request(HttpMethod.Patch, route);
        }

        public IFluffRequest Put(string route)
        {
            return Request(HttpMethod.Put, route);
        }

        public IFluffRequest Delete(string route)
        {
            return Request(HttpMethod.Delete, route);
        }

        public IFluffRequest Request(HttpMethod method, string route)
        {
            return new FluffRequest(this, method, route);
        }

        public async Task<T> ExecAsync<T>(HttpRequestMessage buildedMessage, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage result = null;

            try
            {
                result = await _httpClient.SendAsync(buildedMessage, cancellationToken);

                if (_settings.EnsureSuccessCode)
                {
                    result.EnsureSuccessStatusCode();
                }

                var contentStream = await result.Content.ReadAsStreamAsync();
                T objectResult = await JsonSerializer.DeserializeAsync<T>(contentStream, cancellationToken: cancellationToken);

                return objectResult;
            }
            catch (HttpRequestException httpEx)
            {
                var stringContent = await result.Content.ReadAsStringAsync();
                throw new FluffRequestException("Unhandled exception occured during processing of request", stringContent, httpEx);
            }
        }
    }
}
