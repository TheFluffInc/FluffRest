using FluffRest.Listener;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FluffRestTest.Mocks
{
    public class MockInterceptCancellationTokenListener : IFluffListener
    {
        public CancellationToken Token { get; private set; }

        public Task OnRequestHttpFailedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            Token = cancellationToken;
            return Task.CompletedTask;
        }

        public Task OnRequestReceivedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            Token = cancellationToken;
            return Task.CompletedTask;
        }

        public Task<HttpRequestMessage> OnRequestSentAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Token = cancellationToken;
            return Task.FromResult(request);
        }
    }
}
