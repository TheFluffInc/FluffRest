using FluffRest.Client;
using FluffRest.Exception;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        internal FluffRequest(IFluffRestClient client, HttpMethod method, string route)
        {
            _client = client;
            _method = method;
            _route = route;
            _parameters = new Dictionary<string, string>();
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

        #region Exceution

        public Task<T> ExecAsync<T>(CancellationToken cancellationToken = default)
        {
            return _client.ExecAsync<T>(BuildRequest(), cancellationToken);
        }


        #endregion

        #region Private

        private HttpRequestMessage BuildRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage(_method, BuildRequestUrl());
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