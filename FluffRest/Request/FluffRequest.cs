using FluffRest.Client;
using FluffRest.Exception;
using FluffRest.Request.Advanced;
using FluffRest.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace FluffRest.Request
{
    internal class FluffRequest : IFluffRequest
    {
        private const int DefaultStringBuilderCapacity = 512;
        private readonly IFluffRestClient _client;
        private readonly HttpMethod _method;
        private readonly string _route;
        private readonly Dictionary<string, string> _parameters;
        private readonly Dictionary<string, string> _headers;
        private object _body;
        private bool _isRawBody;
        private string _rawBodyContentType;
        private string _cancellationKey;

        internal FluffRequest(IFluffRestClient client, HttpMethod method, string route, string cancellationKey = null)
        {
            _client = client;
            _method = method;
            _route = route;
            _parameters = new Dictionary<string, string>();
            _headers = new Dictionary<string, string>();
            _body = null;
            _cancellationKey = cancellationKey;
        }

        #region Query Parameters

        public IFluffRequest AddQueryParameter(string key, string value)
        {
            if (!_parameters.ContainsKey(key))
            {
                _parameters.Add(key, value);
            }
            else
            {
                if (_client.Settings.DuplicateParameterKeyHandling == Settings.FluffDuplicateParameterKeyHandling.Throw)
                {
                    throw new FluffDuplicateParameterException($"Trying to add duplicate key '{key}' in query paramters, either remove duplicate or configure the client");
                }
                else if (_client.Settings.DuplicateParameterKeyHandling == Settings.FluffDuplicateParameterKeyHandling.Replace)
                {
                    _parameters[key] = value;
                }
            }

            return this;
        }

        public IFluffRequest AddQueryParameter(string key, int value)
        {
            return AddQueryParameter(key, value.ToString());
        }

        public IFluffRequest AddQueryParameter(string key, short value)
        {
            return AddQueryParameter(key, value.ToString());
        }

        public IFluffRequest AddQueryParameter(string key, long value)
        {
            return AddQueryParameter(key, value.ToString());
        }

        #endregion

        #region Headers

        public IFluffRequest AddHeader(string key, string value)
        {
            if (!_headers.ContainsKey(key))
            {
                _headers.Add(key, value);
            }
            else
            {
                if (_client.Settings.DuplicateHeaderHandling == FluffDuplicateHeaderHandling.Throw)
                {
                    throw new FluffDuplicateParameterException($"Duplicate default header with key '{key}'");
                }
                else if (_client.Settings.DuplicateHeaderHandling == FluffDuplicateHeaderHandling.Replace)
                {
                    _headers[key] = value;
                }
            }

            return this;
        }

        #endregion

        #region Body

        public IFluffRequest AddBody<T>(T body)
        {
            if (_body != null)
            {
                throw new InvalidOperationException("You can only have one body per request");
            }

            _body = body;
            _isRawBody = false;
            return this;
        }

        public IFluffRequest AddBody(string rawBody, string contentType = "application/json")
        {
            if (_body != null)
            {
                throw new InvalidOperationException("You can only have one body per request");
            }

            _body = rawBody;
            _rawBodyContentType = contentType;
            _isRawBody = true;
            return this;
        }

        #endregion

        #region Cancellation Token

        public IFluffRequest WithAutoCancellation(string cancellationKey = "default")
        {
            _cancellationKey = cancellationKey;
            return this;
        }

        public bool IsAutoCancellationConfigured()
        {
            return !string.IsNullOrEmpty(_cancellationKey);
        }

        #endregion

        #region Execution

        public async Task<T> ExecAsync<T>(CancellationToken cancellationToken = default)
        {
            var request = await BuildRequestAsync("application/json", cancellationToken);
            return await _client.ExecAsync<T>(request, GetCancellationFromKeyOrProvidedOne(cancellationToken));
        }

        public async Task ExecAsync(CancellationToken cancellationToken = default)
        {
            var request = await BuildRequestAsync(cancellationToken: cancellationToken);
            await _client.ExecAsync(request, GetCancellationFromKeyOrProvidedOne(cancellationToken));
        }

        public async Task<string> ExecStringAsync(CancellationToken cancellationToken = default)
        {
            var request = await BuildRequestAsync(cancellationToken: cancellationToken);
            return await _client.ExecStringAsync(request, GetCancellationFromKeyOrProvidedOne(cancellationToken));
        }

        public async Task<FluffAdvancedResponse<T>> ExecAdvancedAsync<T>(CancellationToken cancellationToken = default)
        {
            var request = await BuildRequestAsync(cancellationToken: cancellationToken);
            return await _client.ExecAdvancedAsync<T>(request, GetCancellationFromKeyOrProvidedOne(cancellationToken));
        }

        #endregion

        #region Private

        private async Task<HttpRequestMessage> BuildRequestAsync(string accept = null, CancellationToken cancellationToken = default)
        {
            HttpRequestMessage request = new HttpRequestMessage(_method, BuildRequestUrl());

            if (_client.DefaultHeaders.Any())
            {
                for (int i = 0; i < _client.DefaultHeaders.Count; i++)
                {
                    var header = _client.DefaultHeaders.ElementAt(i);
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (_headers.Any())
            {
                for (int i = 0; i < _headers.Count; i++)
                {
                    var header = _headers.ElementAt(i);

                    if (request.Headers.Any(x => x.Key == header.Key))
                    {
                        if (_client.Settings.DuplicateDefaultHeaderHandling == FluffDuplicateWithDefaultHeaderHandling.Throw)
                        {
                            throw new FluffDuplicateParameterException($"Conflicting request header with default one '{header.Key}', fix it or configure client to change this behavior.");
                        }
                        else if (_client.Settings.DuplicateDefaultHeaderHandling == FluffDuplicateWithDefaultHeaderHandling.Replace)
                        {
                            request.Headers.Remove(header.Key);
                            request.Headers.Add(header.Key, header.Value);
                        }
                    }
                    else
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }
            }

            if (!string.IsNullOrEmpty(accept) && !request.Headers.Any(x => x.Key == "Accept"))
            {
                request.Headers.Add("Accept", accept);
            }

            if (_body != null)
            {
                if (_isRawBody)
                {
                    request.Content = new StringContent((string)_body, Encoding.UTF8, _rawBodyContentType);
                }
                else
                {
                    var json = await _client.Serializer.SerializeAsync(_body, cancellationToken);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

            }

            return request;
        }

        private string BuildRequestUrl()
        {
            StringBuilder finalUrl = new StringBuilder(_client.BaseUrl.TrimEnd('/'), DefaultStringBuilderCapacity);

            finalUrl.Append('/');
            finalUrl.Append(_route.TrimEnd('/'));

            if (_parameters.Count > 0)
            {
                finalUrl.Append("?");

                for (int i = 0; i < _parameters.Count; i++)
                {
                    var param = _parameters.ElementAt(i);
                    finalUrl.Append(HttpUtility.UrlEncode(param.Key));
                    finalUrl.Append('=');
                    finalUrl.Append(HttpUtility.UrlEncode(param.Value));

                    if (i < _parameters.Count - 1)
                    {
                        finalUrl.Append('&');
                    }
                }
            }

            return finalUrl.ToString();
        }

        private CancellationToken GetCancellationFromKeyOrProvidedOne(CancellationToken providedToken)
        {
            if (providedToken != default)
            {
                return providedToken;
            }
            else if (!string.IsNullOrEmpty(_cancellationKey))
            {
                return _client.GetCancellationFromKey(_cancellationKey);
            }

            return default;
        }

        #endregion
    }
}