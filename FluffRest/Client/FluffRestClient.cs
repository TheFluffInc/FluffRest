using FluffRest.Exception;
using FluffRest.Listener;
using FluffRest.Request;
using FluffRest.Request.Advanced;
using FluffRest.Serializer;
using FluffRest.Settings;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly List<IFluffListener> _listeners;
        private readonly Dictionary<string, CancellationTokenSource> _cancellationTokens;
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public IFluffRestClient AddBasicAuth(string username, string password)
        {
            var plainCredentials = $"{username}:{password}";
            var plainCredentialsBytes = System.Text.Encoding.UTF8.GetBytes(plainCredentials);
            var base64Credientials = System.Convert.ToBase64String(plainCredentialsBytes);
            AddDefaultHeader(AuthorizationHeader, $"Basic {base64Credientials}");
            return this;
        }

        /// <inheritdoc/>
        public IFluffRestClient AddBearerAuth(string token)
        {
            AddDefaultHeader(AuthorizationHeader, $"Bearer {token}");
            return this;
        }

        /// <inheritdoc/>
        public IFluffRestClient AddAuth(string scheme, string value)
        {
            var authHeader = $"{scheme} {value}";
            return AddDefaultHeader(AuthorizationHeader, authHeader);
        }

        #endregion

        #region Default Parameters

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public IFluffRestClient AddDefaultQueryParameter(string key, int value)
        {
            return AddDefaultQueryParameter(key, value.ToString());
        }

        /// <inheritdoc/>
        public IFluffRestClient AddDefaultQueryParameter(string key, short value)
        {
            return AddDefaultQueryParameter(key, value.ToString());
        }

        /// <inheritdoc/>
        public IFluffRestClient AddDefaultQueryParameter(string key, long value)
        {
            return AddDefaultQueryParameter(key, value.ToString());
        }

        /// <inheritdoc/>
        public IFluffRestClient AddDefaultQueryParameter(string key, decimal value)
        {
            return AddDefaultQueryParameter(key, value.ToString());
        }

        /// <inheritdoc/>
        public IFluffRestClient AddDefaultQueryParameter(string key, int? value)
        {
            return AddDefaultQueryParameter(key, value?.ToString());
        }

        /// <inheritdoc/>
        public IFluffRestClient AddDefaultQueryParameter(string key, decimal? value)
        {
            return AddDefaultQueryParameter(key, value?.ToString());
        }

        /// <inheritdoc/>
        public IFluffRestClient AddDefaultQueryParameter(string key, short? value)
        {
            return AddDefaultQueryParameter(key, value?.ToString());
        }

        /// <inheritdoc/>
        public IFluffRestClient AddDefaultQueryParameter(string key, long? value)
        {
            return AddDefaultQueryParameter(key, value?.ToString());
        }

        #endregion

        #region Request

        /// <inheritdoc/>
        public IFluffRequest Get(string route)
        {
            return Request(HttpMethod.Get, route);
        }

        /// <inheritdoc/>
        public IFluffRequest Post(string route)
        {
            return Request(HttpMethod.Post, route);
        }

        /// <inheritdoc/>
        public IFluffRequest Patch(string route)
        {
            return Request(HttpMethod.Patch, route);
        }

        /// <inheritdoc/>
        public IFluffRequest Put(string route)
        {
            return Request(HttpMethod.Put, route);
        }

        /// <inheritdoc/>
        public IFluffRequest Delete(string route)
        {
            return Request(HttpMethod.Delete, route);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public IFluffRestClient RegisterListener(IFluffListener listener)
        {
            _listeners.Add(listener);
            return this;
        }

        #endregion

        #region Cancellation Token

        /// <inheritdoc/>
        public IFluffRestClient WithAutoCancellation()
        {
            _useAutoCancel = true;
            return this;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
                
                if (contentStream.Length > 0)
                {
                    T objectResult = await _serializer.DeserializeAsync<T>(contentStream, cancellationToken);
                    return objectResult;
                }
                else
                {
                    return default;
                }
            }
            catch (HttpRequestException httpEx)
            {
                await CallRequestFailedListenersAsync(result, cancellationToken);
                var stringContent = await result.Content.ReadAsStringAsync();
                throw new FluffRequestException("Unhandled exception occured during processing of request", stringContent, result.StatusCode, _serializer, httpEx);
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

                if (contentStream.Length > 0)
                {
                    T objectResult = await _serializer.DeserializeAsync<T>(contentStream, cancellationToken);
                    return new FluffAdvancedResponse<T>(objectResult, result.StatusCode);
                }
                else
                {
                    return new FluffAdvancedResponse<T>(default, result.StatusCode);
                }
            }
            catch (HttpRequestException httpEx)
            {
                await CallRequestFailedListenersAsync(result, cancellationToken);
                var stringContent = await result.Content.ReadAsStringAsync();
                throw new FluffRequestException("Unhandled exception occured during processing of request", stringContent, result.StatusCode, _serializer, httpEx);
            }
        }

        /// <inheritdoc/>
        public async Task<FluffAdvancedResponse> ExecAdvancedRawAsync(HttpRequestMessage buildedMessage, CancellationToken cancellationToken = default)
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

                if (contentStream.Length > 0)
                {
                    var contentString = await result.Content.ReadAsStringAsync();
                    return new FluffAdvancedResponse(contentString, result.StatusCode, _serializer);
                }
                else
                {
                    return new FluffAdvancedResponse(null, result.StatusCode, _serializer);
                }
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
                for (int i = 0; i < _listeners.Count; i++)
                {
                    request = await _listeners[i].OnRequestSentAsync(request, cancellationToken);
                }
            }

            return request;
        }

        private async Task CallAfterRequestListenersAsync(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
        {
            if (_listeners.Any())
            {
                for (int i = 0; i < _listeners.Count; i++)
                {
                    await _listeners[i].OnRequestReceivedAsync(httpResponseMessage, cancellationToken);
                }
            }
        }

        private async Task CallRequestFailedListenersAsync(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
        {
            if (_listeners.Any())
            {
                for (int i = 0; i < _listeners.Count; i++)
                {
                    await _listeners[i].OnRequestHttpFailedAsync(httpResponseMessage, cancellationToken);
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
