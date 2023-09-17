using FluffRest.Listener;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FluffRestTest.Mocks
{
    public class MockListener : IFluffListener
    {
        public bool IsRequestSentCalled { get; private set; }
        public bool IsAfterRequestCalled { get; private set; }
        public bool IsRequestFailedCalled { get; private set; }

        public HttpResponseMessage LastResponse { get; private set; }
        public HttpRequestMessage LastRequest { get; private set; }

        public Task OnRequestHttpFailedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            IsRequestFailedCalled = true;
            LastResponse = response;
            return Task.CompletedTask;
        }

        public Task OnRequestReceivedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            IsAfterRequestCalled = true;
            LastResponse = response;
            return Task.CompletedTask;
        }

        public Task<HttpRequestMessage> OnRequestSentAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            IsRequestSentCalled = true;
            request.Headers.Add("x-test-listner", "true");
            LastRequest = request;
            return Task.FromResult(request);
        }
    }
}
