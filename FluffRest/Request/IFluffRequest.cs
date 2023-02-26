using System.Threading.Tasks;
using System.Threading;

namespace FluffRest.Request
{
    public interface IFluffRequest
    {
        /// <summary>
        /// Execute the request and automatically perform json deserialization.
        /// </summary>
        /// <typeparam name="T">Type to map json received.</typeparam>
        /// <param name="cancellationToken">Cancellation token to forward.</param>
        /// <returns>Deserilized response from api.</returns>
        Task<T> ExecAsync<T>(CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute the request.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to forward.</param>
        /// <returns></returns>
        Task ExecAsync(CancellationToken cancellationToken = default);

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
    }
}
