using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluffRest.Listener
{
    public interface IFluffListener
    {
        /// <summary>
        /// Will be called before request is sent. Here you can override request being sent by editing request message and returning edited one.
        /// Do not forget to return a message.
        /// </summary>
        /// <param name="request">Request that will be sent out.</param>
        /// <param name="cancellationToken">Cancellation token to forward.</param>
        /// <returns></returns>
        Task<HttpRequestMessage> OnRequestSentAsync(HttpRequestMessage request, CancellationToken cancellationToken);

        /// <summary>
        /// Will be called after response received if successful (or in every case if settings disable success check).
        /// </summary>
        /// <param name="response">Reponse received.</param>
        /// <param name="cancellationToken">Cancellation token to forward.</param>
        /// <returns></returns>
        Task OnRequestReceivedAsync(HttpResponseMessage response, CancellationToken cancellationToken);

        /// <summary>
        /// Will be called after response http code check fails.
        /// </summary>
        /// <param name="response">Reponse received.</param>
        /// <param name="cancellationToken">Cancellation token to forward.</param>
        /// <returns></returns>
        Task OnRequestHttpFailedAsync(HttpResponseMessage response, CancellationToken cancellationToken);
    }
}
