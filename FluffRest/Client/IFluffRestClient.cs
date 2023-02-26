using FluffRest.Request;
using FluffRest.Settings;
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
    }
}
