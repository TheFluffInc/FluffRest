using FluffRest.Compression;
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
using System.Text;
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
        private readonly List<IFluffCompressor> _compressors;
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
            _compressors = new List<IFluffCompressor>();
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

        #region Compression

        /// <inheritdoc/>
        public IFluffRestClient RegisterCompression(IFluffCompressor compressor)
        {
            if (_compressors.Exists(c => c.AcceptHeaderName == compressor.AcceptHeaderName))
            {
                throw new InvalidOperationException($"A register for {compressor.AcceptHeaderName} is already registred");
            }

            _compressors.Add(compressor);
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
            var result = await InternalRequestExecuteAsync(buildedMessage, cancellationToken);
            var content = await GetBodyOfResponseAsync(result, cancellationToken);

            if (content.Length > 0)
            {
                using var contentStream = new MemoryStream(content);
                T objectResult = await _serializer.DeserializeAsync<T>(contentStream, cancellationToken);
                return objectResult;
            }
            else
            {
                return default;
            }
        }

        /// <inheritdoc/>
        public Task ExecAsync(HttpRequestMessage buildedMessage, CancellationToken cancellationToken = default)
        {
            return InternalRequestExecuteAsync(buildedMessage, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<string> ExecStringAsync(HttpRequestMessage buildedMessage, CancellationToken cancellationToken = default)
        {
            var result = await InternalRequestExecuteAsync(buildedMessage, cancellationToken);
            var content = await GetBodyOfResponseAsync(result, cancellationToken);
            return Encoding.UTF8.GetString(content);
        }

        /// <inheritdoc/>
        public async Task<FluffAdvancedResponse<T>> ExecAdvancedAsync<T>(HttpRequestMessage buildedMessage, CancellationToken cancellationToken = default)
        {
            var result = await InternalRequestExecuteAsync(buildedMessage, cancellationToken);
            var content = await GetBodyOfResponseAsync(result, cancellationToken);

            if (content.Length > 0)
            {
                using var contentStream = new MemoryStream(content);
                T objectResult = await _serializer.DeserializeAsync<T>(contentStream, cancellationToken);
                return new FluffAdvancedResponse<T>(objectResult, result.StatusCode);
            }
            else
            {
                return new FluffAdvancedResponse<T>(default, result.StatusCode);
            }
        }

        /// <inheritdoc/>
        public async Task<FluffAdvancedResponse> ExecAdvancedRawAsync(HttpRequestMessage buildedMessage, CancellationToken cancellationToken = default)
        {
            var result = await InternalRequestExecuteAsync(buildedMessage, cancellationToken);
            var content = await GetBodyOfResponseAsync(result, cancellationToken);

            if (content.Length > 0)
            {
                return new FluffAdvancedResponse(Encoding.UTF8.GetString(content), result.StatusCode, _serializer);
            }
            else
            {
                return new FluffAdvancedResponse(null, result.StatusCode, _serializer);
            }
        }

        #endregion

        #region Private

        private async Task<byte[]> GetBodyOfResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.Content.Headers.ContentEncoding.Count > 0)
            {
                var encoding = response.Content.Headers.ContentEncoding.First();

                var suitableCompressor = _compressors.FirstOrDefault(x => x.AcceptHeaderName == encoding);

                if (suitableCompressor == null)
                {
                    throw new InvalidOperationException($"No compressor defined for encoding {encoding}, please add it to support this request");
                }

                using var httpStream = await response.Content.ReadAsStreamAsync();
                return await suitableCompressor.DecompressAsync(httpStream, cancellationToken);
            }
            else
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
        }

        private async Task<HttpResponseMessage> InternalRequestExecuteAsync(HttpRequestMessage buildedMessage, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage result = null;

            try
            {
                var acceptedEncodings = string.Join(", ", _compressors.Select(x => x.AcceptHeaderName));
                buildedMessage.Headers.Add("Accept-Encoding", acceptedEncodings);

                buildedMessage = await CallBeforeSendListenersAsync(buildedMessage, cancellationToken);
                result = await _httpClient.SendAsync(buildedMessage, cancellationToken);

                if (_settings.EnsureSuccessCode)
                {
                    result.EnsureSuccessStatusCode();
                }

                await CallAfterRequestListenersAsync(result, cancellationToken);

                return result;
            }
            catch (HttpRequestException ex)
            {
                await CallRequestFailedListenersAsync(result, cancellationToken);
                var stringContent = result == null ? null : await result.Content.ReadAsStringAsync();
                throw new FluffRequestException("Unhandled exception occured during processing of request", stringContent, result?.StatusCode ?? default, _serializer, ex);
            }

        }

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
