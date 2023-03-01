using FluffRest.Listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluffRestTest.Mocks
{
    public class MockListener : IFluffListener
    {
        public bool IsRequestSentCalled { get; private set; }
        public bool IsAfterRequestCalled { get; private set; }
        public bool IsRequestFailedCalled { get; private set; }

        public Task OnRequestHttpFailedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            IsRequestFailedCalled = true;
            return Task.CompletedTask;
        }

        public Task OnRequestReceivedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            IsAfterRequestCalled = true;
            return Task.CompletedTask;
        }

        public Task<HttpRequestMessage> OnRequestSentAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            IsRequestSentCalled = true;
            request.Headers.Add("x-test-listner", "true");
            return Task.FromResult(request);
        }
    }
}
