using FluffRest.Client;
using FluffRest.Exception;
using FluffRest.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        internal FluffRequest(IFluffRestClient client, HttpMethod method, string route)
        {
            _client = client;
            _method = method;
            _route = route;
            _parameters = new Dictionary<string, string>();
            _headers = new Dictionary<string, string>();
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

        #region Execution

        public Task<T> ExecAsync<T>(CancellationToken cancellationToken = default)
        {
            return _client.ExecAsync<T>(BuildRequest("application/json"), cancellationToken);
        }

        public Task ExecAsync(CancellationToken cancellationToken = default)
        {
            return _client.ExecAsync(BuildRequest(), cancellationToken);
        }

        #endregion

        #region Private

        private HttpRequestMessage BuildRequest(string accept = null)
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
                    finalUrl.Append(param.Key);
                    finalUrl.Append('=');
                    finalUrl.Append(param.Value);

                    if (i < _parameters.Count - 1)
                    {
                        finalUrl.Append('&');
                    }
                }
            }

            return finalUrl.ToString();
        }

        #endregion
    }
}