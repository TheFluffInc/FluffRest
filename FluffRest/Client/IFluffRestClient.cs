using FluffRest.Compression;
using FluffRest.Listener;
using FluffRest.Request;
using FluffRest.Request.Advanced;
using FluffRest.Serializer;
using FluffRest.Settings;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FluffRest.Client
{
    public interface IFluffRestClient
    {
        /// <summary>
        /// Base url reference for api calls.
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// Settings to be used globally by requests. <see cref="FluffClientSettings"/>
        /// </summary>
        FluffClientSettings Settings { get; }

        IFluffSerializer Serializer { get; }

        /// <summary>
        /// DO NOT TOUCH THIS, use method add default header
        /// </summary>
        Dictionary<string, string> DefaultHeaders { get; }

        /// <summary>
        /// Add a default header that will be added in all request made using this client.
        /// </summary>
        /// <param name="key">Name of the header</param>
        /// <param name="value">Value of the header</param>
        /// <returns></returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Will throw if you add duplicate header keys.</exception>
        IFluffRestClient AddDefaultHeader(string key, string value);

        /// <summary>
        /// Create the Authorization header using basic scheme that will be sent with all request made by this client.
        /// </summary>
        /// <param name="username">plain username</param>
        /// <param name="password">plain password</param>
        /// <returns></returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Will throw if authentication is already set.</exception>
        IFluffRestClient AddBasicAuth(string username, string password);

        /// <summary>
        /// Create the Authorization header using bearer scheme that will be sent with all request made by this client.
        /// </summary>
        /// <param name="token">Bearer token to send</param>
        /// <returns></returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Will throw if authentication is already set.</exception>
        IFluffRestClient AddBearerAuth(string token);

        /// <summary>
        /// Create the Authorization header using custom scheme that will be sent with all request made by this client.
        /// </summary>
        /// <param name="scheme">Auth scheme to use.</param>
        /// <param name="value">Auth value to send</param>
        /// <returns></returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Will throw if authentication is already set.</exception>
        IFluffRestClient AddAuth(string scheme, string value);

        /// <summary>
        /// Add a default query parameter to the url path.
        /// </summary>
        /// <param name="key">key of the parameter</param>
        /// <param name="value">value of the parameter</param>
        /// <returns>Client configured with this parameter.</returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Depending of the configuration of the client, duplicate keys will throw or have different behaviour, configure <see cref="Settings.FluffClientSettings"/> to override default settings.</exception>
        IFluffRestClient AddDefaultQueryParameter(string key, string value);

        /// <summary>
        /// Add a default query parameter to the url path.
        /// </summary>
        /// <param name="key">key of the parameter</param>
        /// <param name="value">value of the parameter</param>
        /// <returns>Client configured with this parameter.</returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Depending of the configuration of the client, duplicate keys will throw or have different behaviour, configure <see cref="Settings.FluffClientSettings"/> to override default settings.</exception>
        IFluffRestClient AddDefaultQueryParameter(string key, int value);

        /// <summary>
        /// Add a default query parameter to the url path.
        /// </summary>
        /// <param name="key">key of the parameter</param>
        /// <param name="value">value of the parameter</param>
        /// <returns>Client configured with this parameter.</returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Depending of the configuration of the client, duplicate keys will throw or have different behaviour, configure <see cref="Settings.FluffClientSettings"/> to override default settings.</exception>
        IFluffRestClient AddDefaultQueryParameter(string key, short value);

        /// <summary>
        /// Add a default query parameter to the url path.
        /// </summary>
        /// <param name="key">key of the parameter</param>
        /// <param name="value">value of the parameter</param>
        /// <returns>Client configured with this parameter.</returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Depending of the configuration of the client, duplicate keys will throw or have different behaviour, configure <see cref="Settings.FluffClientSettings"/> to override default settings.</exception>
        IFluffRestClient AddDefaultQueryParameter(string key, long value);

        /// <summary>
        /// Add a default query parameter to the url path.
        /// </summary>
        /// <param name="key">key of the parameter</param>
        /// <param name="value">value of the parameter</param>
        /// <returns>Client configured with this parameter.</returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Depending of the configuration of the client, duplicate keys will throw or have different behaviour, configure <see cref="Settings.FluffClientSettings"/> to override default settings.</exception>
        IFluffRestClient AddDefaultQueryParameter(string key, decimal value);

        /// <summary>
        /// Add a default query parameter to the url path.
        /// </summary>
        /// <param name="key">key of the parameter</param>
        /// <param name="value">value of the parameter</param>
        /// <returns>Client configured with this parameter.</returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Depending of the configuration of the client, duplicate keys will throw or have different behaviour, configure <see cref="Settings.FluffClientSettings"/> to override default settings.</exception>
        IFluffRestClient AddDefaultQueryParameter(string key, int? value);

        /// <summary>
        /// Add a default query parameter to the url path.
        /// </summary>
        /// <param name="key">key of the parameter</param>
        /// <param name="value">value of the parameter</param>
        /// <returns>Client configured with this parameter.</returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Depending of the configuration of the client, duplicate keys will throw or have different behaviour, configure <see cref="Settings.FluffClientSettings"/> to override default settings.</exception>
        IFluffRestClient AddDefaultQueryParameter(string key, decimal? value);

        /// <summary>
        /// Add a default query parameter to the url path.
        /// </summary>
        /// <param name="key">key of the parameter</param>
        /// <param name="value">value of the parameter</param>
        /// <returns>Client configured with this parameter.</returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Depending of the configuration of the client, duplicate keys will throw or have different behaviour, configure <see cref="Settings.FluffClientSettings"/> to override default settings.</exception>
        IFluffRestClient AddDefaultQueryParameter(string key, short? value);

        /// <summary>
        /// Add a default query parameter to the url path.
        /// </summary>
        /// <param name="key">key of the parameter</param>
        /// <param name="value">value of the parameter</param>
        /// <returns>Client configured with this parameter.</returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Depending of the configuration of the client, duplicate keys will throw or have different behaviour, configure <see cref="Settings.FluffClientSettings"/> to override default settings.</exception>
        IFluffRestClient AddDefaultQueryParameter(string key, long? value);

        /// <summary>
        /// Register a listner to this client.
        /// </summary>
        /// <param name="listener">Listner that will receive instructions.</param>
        /// <returns></returns>
        IFluffRestClient RegisterListener(IFluffListener listener);

        /// <summary>
        /// Register a compression that the client can use.
        /// </summary>
        /// <param name="compressor">Compressor implementation to register.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the accept header is already used by another implementation.</exception>
        IFluffRestClient RegisterCompression(IFluffCompressor compressor);

        /// <summary>
        /// Make all requests use an automatic cancellation of request. Configure which tokens are sent to the requests by editing the settings <see cref="FluffClientSettings"/>
        /// </summary>
        /// <returns></returns>
        IFluffRestClient WithAutoCancellation();

        /// <summary>
        /// Cancell all requests made with the client.
        /// </summary>
        void CancellAllRequests();

        /// <summary>
        /// Get a ressource to endpoint.
        /// </summary>
        /// <param name="route">Route segment to endpoint excluding base url.</param>
        /// <returns></returns>
        IFluffRequest Get(string route);

        /// <summary>
        /// Post a ressource to endpoint.
        /// </summary>
        /// <param name="route">Route segment to endpoint excluding base url.</param>
        /// <returns></returns>
        IFluffRequest Post(string route);

        /// <summary>
        /// Patch a ressource to endpoint.
        /// </summary>
        /// <param name="route">Route segment to endpoint excluding base url.</param>
        /// <returns></returns>
        IFluffRequest Patch(string route);

        /// <summary>
        /// Put a ressource to endpoint.
        /// </summary>
        /// <param name="route">Route segment to endpoint excluding base url.</param>
        /// <returns></returns>
        IFluffRequest Put(string route);

        /// <summary>
        /// Delete a ressource to endpoint.
        /// </summary>
        /// <param name="route">Route segment to endpoint excluding base url.</param>
        /// <returns></returns>
        IFluffRequest Delete(string route);

        /// <summary>
        /// Custom http verb request initiator, for basic usage you should use other methods.
        /// </summary>
        /// <param name="method">Http method to be used.</param>
        /// <param name="route">Route segment to endpoint excluding base url.</param>
        /// <returns></returns>
        IFluffRequest Request(HttpMethod method, string route);

        /// <summary>
        /// Excute a builded request asyncronously.
        /// </summary>
        /// <typeparam name="T">Type for automatic json deserialization.</typeparam>
        /// <param name="buildedMessage">Builded http message.</param>
        /// <param name="cancellationToken">Cancellation token to be forwarded.</param>
        /// <returns></returns>
        Task<T> ExecAsync<T>(HttpRequestMessage buildedMessage, CancellationToken cancellationToken = default);

        /// <summary>
        /// Excute a builded request asyncronously.
        /// </summary>
        /// <typeparam name="T">Type for automatic json deserialization.</typeparam>
        /// <param name="buildedMessage">Builded http message.</param>
        /// <param name="cancellationToken">Cancellation token to be forwarded.</param>
        /// <returns></returns>
        Task ExecAsync(HttpRequestMessage buildedMessage, CancellationToken cancellationToken = default);

        /// <summary>
        /// Excute a builded request asyncronously.
        /// </summary>
        /// <param name="buildedMessage">Builded http message.</param>
        /// <param name="cancellationToken">Cancellation token to be forwarded.</param>
        /// <returns>Raw string result</returns>
        Task<string> ExecStringAsync(HttpRequestMessage buildedMessage, CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute a builded request asyncronously and return advanced response.
        /// </summary>
        /// <typeparam name="T">Type for automatic json deserialization.</typeparam>
        /// <param name="cancellationToken">Cancellation token to be forwarded.</param>
        /// <returns></returns>
        Task<FluffAdvancedResponse<T>> ExecAdvancedAsync<T>(HttpRequestMessage buildedMessage, CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute a builded request asyncronously and return advanced response.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to be forwarded.</param>
        /// <returns></returns>
        Task<FluffAdvancedResponse> ExecAdvancedRawAsync(HttpRequestMessage buildedMessage, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get and cancel previously allocated cancellation token for this key.
        /// Do not call this method direclty, call method on request configuration.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        CancellationToken GetCancellationFromKey(string key);
    }
}
