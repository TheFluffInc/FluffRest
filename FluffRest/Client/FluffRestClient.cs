using FluffRest.Exception;
using FluffRest.Listener;
using FluffRest.Request;
using FluffRest.Request.Advanced;
using FluffRest.Serializer;
using FluffRest.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FluffRest.Client
{
    public class FluffRestClient : IFluffRestClient, IDisposable
    {
        private const string AuthorizationHeader = "Authorization";
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        private readonly FluffClientSettings _settings;
        private readonly Dictionary<string, string> _defaultHeaders;
        private readonly IFluffSerializer _serializer;
        private readonly Dictionary<string, string> _defaultParameters;
        private List<IFluffListener> _listeners;
        private Dictionary<string, CancellationTokenSource> _cancellationTokens;
        private bool _useAutoCancel;
        private bool _disposedValue;

        /// <summary>
        /// Create a new rest client, using an existing HttpClient. It can be configured via the <see cref="FluffClientSettings"/> object.
        /// </summary>
        /// <param name="baseUrl">Root url of your api, you will specify endpoints later.</param>
        /// <param name="httpClient">HttpClient from your app</param>
        /// <param name="settings">Optional settings for deep configuration see <see cref="FluffClientSettings"/> object</param>
        /// <exception cref="System.ArgumentException">If the base url is null or empty or if the http client is null</exception>
        public FluffRestClient(string baseUrl, HttpClient httpClient, FluffClientSettings settings = null, IFluffSerializer serializer = null)
        {
            _baseUrl = !string.IsNullOrEmpty(baseUrl) ? baseUrl : throw new System.ArgumentException($"{nameof(baseUrl)} is null or empty, please provide a base url for api calls");
            _httpClient = httpClient ?? throw new System.ArgumentException($"{nameof(httpClient)} is null, please provide a http client");
            _settings = settings ?? new FluffClientSettings();
            _defaultHeaders = new Dictionary<string, string>();
            _listeners = new List<IFluffListener>();
            _cancellationTokens = new Dictionary<string, CancellationTokenSource>();
            _useAutoCancel = false;
            _serializer = serializer ?? new JsonFluffSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web));
            _defaultParameters = new Dictionary<string, string>();
        }

        public string BaseUrl => _baseUrl;

        public FluffClientSettings Settings => _settings;

        public Dictionary<string, string> DefaultHeaders => _defaultHeaders;

        public IFluffSerializer Serializer => _serializer;

        #region Default Headers & Auth

        public IFluffRestClient AddDefaultHeader(string key, string value)
        {
            if (!_defaultHeaders.ContainsKey(key))
            {
                _defaultHeaders.Add(key, value);
            }
            else
            {
                if (_settings.DuplicateHeaderHandling == FluffDuplicateHeaderHandling.Throw)
                {
                    throw new FluffDuplicateParameterException($"Duplicate default header with key '{key}'");
                }
                else if (_settings.DuplicateHeaderHandling == FluffDuplicateHeaderHandling.Replace)
                {
                    _defaultHeaders[key] = value;
                }
            }

            return this;
        }

        public IFluffRestClient AddBasicAuth(string username, string password)
        {
            var plainCredentials = $"{username}:{password}";
            var plainCredentialsBytes = System.Text.Encoding.UTF8.GetBytes(plainCredentials);
            var base64Credientials = System.Convert.ToBase64String(plainCredentialsBytes);
            AddDefaultHeader(AuthorizationHeader, $"Basic {base64Credientials}");
            return this;
        }

        public IFluffRestClient AddBearerAuth(string token)
        {
            AddDefaultHeader(AuthorizationHeader, $"Bearer {token}");
            return this;
        }

        public IFluffRestClient AddAuth(string scheme, string value)
        {
            var authHeader = $"{scheme} {value}";
            return AddDefaultHeader(AuthorizationHeader, authHeader);
        }

        #endregion

        #region Default Parameters

        public IFluffRestClient AddDefaultQueryParameter(string key, string value)
        {
            if (value != null)
            {
                if (!_defaultParameters.ContainsKey(key))
                {
                    _defaultParameters.Add(key, value);
                }
                else
                {
                    if (_settings.DuplicateParameterKeyHandling == FluffDuplicateParameterKeyHandling.Throw)
                    {
                        throw new FluffDuplicateParameterException($"Trying to add duplicate key '{key}' in query paramters, either remove duplicate or configure the client");
                    }
                    else if (_settings.DuplicateParameterKeyHandling == FluffDuplicateParameterKeyHandling.Replace)
                    {
                        _defaultParameters[key] = value;
                    }
                }
            }

            return this;
        }

        public IFluffRestClient AddDefaultQueryParameter(string key, int value)
        {
            return AddDefaultQueryParameter(key, value.ToString());
        }

        public IFluffRestClient AddDefaultQueryParameter(string key, short value)
        {
            return AddDefaultQueryParameter(key, value.ToString());
        }

        public IFluffRestClient AddDefaultQueryParameter(string key, long value)
        {
            return AddDefaultQueryParameter(key, value.ToString());
        }

        public IFluffRestClient AddDefaultQueryParameter(string key, decimal value)
        {
            return AddDefaultQueryParameter(key, value.ToString());
        }

        public IFluffRestClient AddDefaultQueryParameter(string key, int? value)
        {
            return AddDefaultQueryParameter(key, value?.ToString());
        }

        public IFluffRestClient AddDefaultQueryParameter(string key, decimal? value)
        {
            return AddDefaultQueryParameter(key, value?.ToString());
        }

        public IFluffRestClient AddDefaultQueryParameter(string key, short? value)
        {
            return AddDefaultQueryParameter(key, value?.ToString());
        }

        public IFluffRestClient AddDefaultQueryParameter(string key, long? value)
        {
            return AddDefaultQueryParameter(key, value?.ToString());
        }

        #endregion

        #region Request

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
            string cancellationKey = null;

            if (_useAutoCancel)
            {
                if (_settings.AutoCancelHandling == FluffAutoCancelHandling.PerEndpoint)
                {
                    cancellationKey = route;
                }
                else if (_settings.AutoCancelHandling == FluffAutoCancelHandling.PerClient)
                {
                    cancellationKey = "default";
                }
            }

            return new FluffRequest(this, method, route, cancellationKey, _defaultParameters);
        }

        #endregion

        #region Listener

        public IFluffRestClient RegisterListener(IFluffListener listener)
        {
            _listeners.Add(listener);
            return this;
        }

        #endregion

        #region Cancellation Token

        public IFluffRestClient WithAutoCancellation()
        {
            _useAutoCancel = true;
            return this;
        }

        public void CancellAllRequests()
        {
            if (_cancellationTokens.Any())
            {
                for (int i = 0; i < _cancellationTokens.Count; i++)
                {
                    var token = _cancellationTokens.ElementAt(i);
                    token.Value.Cancel();
                    token.Value.Dispose();
                }
            }

            _cancellationTokens.Clear();
        }

        public CancellationToken GetCancellationFromKey(string key)
        {
            if (_cancellationTokens.ContainsKey(key))
            {
                var tokenSource = _cancellationTokens[key];
                tokenSource.Cancel();
                _cancellationTokens.Remove(key);
            }

            var newTokenSource = new CancellationTokenSource();
            _cancellationTokens.Add(key, newTokenSource);
            return newTokenSource.Token;
        }

        #endregion

        #region Exec

        public async Task<T> ExecAsync<T>(HttpRequestMessage buildedMessage, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage result = null;

            try
            {
                buildedMessage = await CallBeforeSendListenersAsync(buildedMessage, cancellationToken);
                result = await _httpClient.SendAsync(buildedMessage, cancellationToken);

                if (_settings.EnsureSuccessCode)
                {
                    result.EnsureSuccessStatusCode();
                }

                await CallAfterRequestListenersAsync(result, cancellationToken);

                var contentStream = await result.Content.ReadAsStreamAsync();
                T objectResult = await _serializer.DeserializeAsync<T>(contentStream, cancellationToken);

                return objectResult;
            }
            catch (HttpRequestException httpEx)
            {
                await CallRequestFailedListenersAsync(result, cancellationToken);
                var stringContent = await result.Content.ReadAsStringAsync();
                throw new FluffRequestException("Unhandled exception occured during processing of request", stringContent, result.StatusCode, _serializer, httpEx);
            }
        }

        public async Task ExecAsync(HttpRequestMessage buildedMessage, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage result = null;

            try
            {
                buildedMessage = await CallBeforeSendListenersAsync(buildedMessage, cancellationToken);
                result = await _httpClient.SendAsync(buildedMessage, cancellationToken);

                if (_settings.EnsureSuccessCode)
                {
                    result.EnsureSuccessStatusCode();
                }

                await CallAfterRequestListenersAsync(result, cancellationToken);
            }
            catch (HttpRequestException httpEx)
            {
                await CallRequestFailedListenersAsync(result, cancellationToken);
                var stringContent = await result.Content.ReadAsStringAsync();
                throw new FluffRequestException("Unhandled exception occured during processing of request", stringContent, result.StatusCode, _serializer, httpEx);
            }
        }

        public async Task<string> ExecStringAsync(HttpRequestMessage buildedMessage, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage result = null;

            try
            {
                buildedMessage = await CallBeforeSendListenersAsync(buildedMessage, cancellationToken);
                result = await _httpClient.SendAsync(buildedMessage, cancellationToken);

                if (_settings.EnsureSuccessCode)
                {
                    result.EnsureSuccessStatusCode();
                }

                await CallAfterRequestListenersAsync(result, cancellationToken);
                var contentString = await result.Content.ReadAsStringAsync();
                return contentString;
            }
            catch (HttpRequestException httpEx)
            {
                await CallRequestFailedListenersAsync(result, cancellationToken);
                var stringContent = await result.Content.ReadAsStringAsync();
                throw new FluffRequestException("Unhandled exception occured during processing of request", stringContent, result.StatusCode, _serializer, httpEx);
            }
        }

        public async Task<FluffAdvancedResponse<T>> ExecAdvancedAsync<T>(HttpRequestMessage buildedMessage, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage result = null;

            try
            {
                buildedMessage = await CallBeforeSendListenersAsync(buildedMessage, cancellationToken);
                result = await _httpClient.SendAsync(buildedMessage, cancellationToken);

                if (_settings.EnsureSuccessCode)
                {
                    result.EnsureSuccessStatusCode();
                }

                await CallAfterRequestListenersAsync(result, cancellationToken);

                var contentStream = await result.Content.ReadAsStreamAsync();
                T objectResult = await _serializer.DeserializeAsync<T>(contentStream, cancellationToken);

                return new FluffAdvancedResponse<T>(objectResult, result.StatusCode);
            }
            catch (HttpRequestException httpEx)
            {
                await CallRequestFailedListenersAsync(result, cancellationToken);
                var stringContent = await result.Content.ReadAsStringAsync();
                throw new FluffRequestException("Unhandled exception occured during processing of request", stringContent, result.StatusCode, _serializer, httpEx);
            }
        }

        #endregion

        #region Private

        private async Task<HttpRequestMessage> CallBeforeSendListenersAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_listeners.Any())
            {
                for (int i = 0; i < _listeners.Count(); i++)
                {
                    request = await _listeners.ElementAt(i).OnRequestSentAsync(request, cancellationToken);
                }
            }

            return request;
        }

        private async Task CallAfterRequestListenersAsync(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
        {
            if (_listeners.Any())
            {
                for (int i = 0; i < _listeners.Count(); i++)
                {
                    await _listeners.ElementAt(i).OnRequestReceivedAsync(httpResponseMessage, cancellationToken);
                }
            }
        }

        private async Task CallRequestFailedListenersAsync(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
        {
            if (_listeners.Any())
            {
                for (int i = 0; i < _listeners.Count(); i++)
                {
                    await _listeners.ElementAt(i).OnRequestHttpFailedAsync(httpResponseMessage, cancellationToken);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    CancellAllRequests();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
