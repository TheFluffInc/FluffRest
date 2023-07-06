using System.Threading.Tasks;
using System.Threading;
using System;
using FluffRest.Request.Advanced;

namespace FluffRest.Request
{
    public interface IFluffRequest
    {
        /// <summary>
        /// Execute the request and automatically perform json deserialization.
        /// </summary>
        /// <typeparam name="T">Type to map json received.</typeparam>
        /// <param name="cancellationToken">Cancellation token to forward, setting this will override configured auto cancellation.</param>
        /// <returns>Deserilized response from api.</returns>
        Task<T> ExecAsync<T>(CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute the request.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to forward, setting this will override configured auto cancellation.</param>
        /// <returns></returns>
        Task ExecAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute the request and get raw string result.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to forward, setting this will override configured auto cancellation.</param>
        /// <returns></returns>
        Task<string> ExecStringAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute the request and return advanced response.
        /// </summary>
        /// <typeparam name="T">Type to map json received.</typeparam>
        /// <param name="cancellationToken">Cancellation token to forward, setting this will override configured auto cancellation.</param>
        /// <returns></returns>
        Task<FluffAdvancedResponse<T>> ExecAdvancedAsync<T>(CancellationToken cancellationToken = default);

        /// <summary>
        /// Add a header only to this request.
        /// </summary>
        /// <param name="key">Name of the header</param>
        /// <param name="value">Value of the header</param>
        /// <returns></returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Depending of the configuration of the client, duplicate headers will throw or have different behaviour, configure <see cref="Settings.FluffClientSettings"/> to override default settings.</exception>
        IFluffRequest AddHeader(string key, string value);

        /// <summary>
        /// Add a query parameter to the url path.
        /// </summary>
        /// <param name="key">key of the parameter</param>
        /// <param name="value">value of the parameter</param>
        /// <returns>Request configured with this parameter.</returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Depending of the configuration of the client, duplicate keys will throw or have different behaviour, configure <see cref="Settings.FluffClientSettings"/> to override default settings.</exception>
        IFluffRequest AddQueryParameter(string key, string value);

        // <summary>
        /// Add a query parameter to the url path.
        /// </summary>
        /// <param name="key">key of the parameter</param>
        /// <param name="value">value of the parameter</param>
        /// <returns>Request configured with this parameter.</returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Depending of the configuration of the client, duplicate keys will throw or have different behaviour, configure <see cref="Settings.FluffClientSettings"/> to override default settings.</exception>
        IFluffRequest AddQueryParameter(string key, int value);

        // <summary>
        /// Add a query parameter to the url path.
        /// </summary>
        /// <param name="key">key of the parameter</param>
        /// <param name="value">value of the parameter</param>
        /// <returns>Request configured with this parameter.</returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Depending of the configuration of the client, duplicate keys will throw or have different behaviour, configure <see cref="Settings.FluffClientSettings"/> to override default settings.</exception>
        IFluffRequest AddQueryParameter(string key, short value);

        // <summary>
        /// Add a query parameter to the url path.
        /// </summary>
        /// <param name="key">key of the parameter</param>
        /// <param name="value">value of the parameter</param>
        /// <returns>Request configured with this parameter.</returns>
        /// <exception cref="Exception.FluffDuplicateParameterException">Depending of the configuration of the client, duplicate keys will throw or have different behaviour, configure <see cref="Settings.FluffClientSettings"/> to override default settings.</exception>
        IFluffRequest AddQueryParameter(string key, long value);

        /// <summary>
        /// Add a json body in the request from a object that will be serialized into json.
        /// </summary>
        /// <typeparam name="T">Type for json serialization.</typeparam>
        /// <param name="body">Object to be serialized.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">You can only have one body per request</exception>
        IFluffRequest AddBody<T>(T body);

        /// <summary>
        /// Add a raw string body in the request, it will not be parsed.
        /// </summary>
        /// <param name="rawBody">Body to be included.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">You can only have one body per request</exception>
        IFluffRequest AddBody(string rawBody, string contentType = "application/json");

        /// <summary>
        /// Configure this request to cancell if another request is made using this client.
        /// </summary>
        /// <param name="cancellationKey">You can specify a 'key' that will be use to separate different excution to allow dictincts cancellation for parrallel tasks.</param>
        /// <returns></returns>
        IFluffRequest WithAutoCancellation(string cancellationKey = "default");

        /// <summary>
        /// Tells you if this request has a cancellation token configured or not.
        /// </summary>
        /// <returns>Is auto cancel of old request enabled?</returns>
        bool IsAutoCancellationConfigured();
    }
}
