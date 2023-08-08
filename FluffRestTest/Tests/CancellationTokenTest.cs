using FluffRest.Client;
using FluffRest.Settings;
using FluffRestTest.Infra;
using FluffRestTest.Mocks;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FluffRestTest.Tests
{
    [TestClass]
    public class CancellationTokenTest : BaseTest
    {
        [TestMethod]
        public void TestCancellationTokenConfiguration()
        {
            var http = GetMockedClient(TestUrl, HttpMethod.Get);
            var client = new FluffRestClient(TestUrl, http);
            var request = client.Get(TestUrl);

            Assert.IsFalse(request.IsAutoCancellationConfigured());

            request = request.WithAutoCancellation();

            Assert.IsTrue(request.IsAutoCancellationConfigured());
        }

        [TestMethod]
        public void TestCancellationTokenProvidedByClient()
        {
            var http = GetMockedClient(TestUrl, HttpMethod.Get);
            var client = new FluffRestClient(TestUrl, http);
            var firstToken = client.GetCancellationFromKey("test");

            Assert.IsFalse(firstToken.IsCancellationRequested);

            var secondToken = client.GetCancellationFromKey("test");

            Assert.IsTrue(firstToken.IsCancellationRequested);
            Assert.IsFalse(secondToken.IsCancellationRequested);
        }

        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task TestProvidedCancellationTokenOverrideAsync()
        {
            var http = GetMockedClient(TestUrl, HttpMethod.Get);
            var client = new FluffRestClient(TestUrl, http);
            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            await client.Get(TestUrl).WithAutoCancellation().ExecAsync(tokenSource.Token);
        }

        [TestMethod]
        public async Task TestGoodCancellationTokenProvidedAsync()
        {
            var url = $"{TestUrl}/cancel";
            var http = GetMockedClient(url, HttpMethod.Get);
            IFluffRestClient client = new FluffRestClient(TestUrl, http);
            var intercepter = new MockInterceptCancellationTokenListener();
            client = client.RegisterListener(intercepter);
            await client.Get("cancel").WithAutoCancellation().ExecAsync();

            var cancellationOfRequest = intercepter.Token;
            await client.Get("cancel").WithAutoCancellation().ExecAsync();

            Assert.IsTrue(cancellationOfRequest.IsCancellationRequested);
        }

        [TestMethod]
        public void TestCancellAllRequests()
        {
            var http = GetMockedClient(TestUrl, HttpMethod.Get);
            var client = new FluffRestClient(TestUrl, http);
            var firstToken = client.GetCancellationFromKey("one");
            var secondToken = client.GetCancellationFromKey("two");
            var thirdToken = client.GetCancellationFromKey("third");

            Assert.IsFalse(firstToken.IsCancellationRequested);
            Assert.IsFalse(secondToken.IsCancellationRequested);
            Assert.IsFalse(thirdToken.IsCancellationRequested);

            client.CancellAllRequests();
            var newFirstToken = client.GetCancellationFromKey("one");

            Assert.IsFalse(newFirstToken.IsCancellationRequested);
            Assert.IsTrue(firstToken.IsCancellationRequested);
            Assert.IsTrue(secondToken.IsCancellationRequested);
            Assert.IsTrue(thirdToken.IsCancellationRequested);
        }

        [TestMethod]
        public void TestAutoCancelConfigureRequests()
        {
            var http = GetMockedClient(TestUrl, HttpMethod.Get);
            IFluffRestClient client = new FluffRestClient(TestUrl, http);
            client = client.WithAutoCancellation();
            var request = client.Get("test");

            Assert.IsTrue(request.IsAutoCancellationConfigured());
        }

        [TestMethod]
        public async Task TestAutoCancelPerRouteAsync()
        {
            var url = $"{TestUrl}/*";
            var http = GetMockedClient(url, HttpMethod.Get);
            var settings = new FluffClientSettings(autoCancelHandling: FluffAutoCancelHandling.PerEndpoint);
            IFluffRestClient client = new FluffRestClient(TestUrl, http, settings);

            var intercepter = new MockInterceptCancellationTokenListener();
            client = client.WithAutoCancellation().RegisterListener(intercepter);

            await client.Get("toto").ExecAsync();
            var firstToken = intercepter.Token;

            await client.Get("tata").ExecAsync();
            var secondToken = intercepter.Token;

            Assert.IsFalse(firstToken.IsCancellationRequested);
            Assert.IsFalse(secondToken.IsCancellationRequested);
        }

        [TestMethod]
        public async Task TestAutoCancelPerClientAsync()
        {
            var url = $"{TestUrl}/*";
            var http = GetMockedClient(url, HttpMethod.Get);
            var settings = new FluffClientSettings(autoCancelHandling: FluffAutoCancelHandling.PerClient);
            IFluffRestClient client = new FluffRestClient(TestUrl, http, settings);

            var intercepter = new MockInterceptCancellationTokenListener();
            client = client.WithAutoCancellation().RegisterListener(intercepter);

            await client.Get("toto").ExecAsync();
            var firstToken = intercepter.Token;

            await client.Get("tata").ExecAsync();
            var secondToken = intercepter.Token;

            Assert.IsTrue(firstToken.IsCancellationRequested);
            Assert.IsFalse(secondToken.IsCancellationRequested);
        }

        [TestMethod]
        public void TestCancellAllRequestOnDispose()
        {
            var http = GetMockedClient(TestUrl, HttpMethod.Get);
            var client = new FluffRestClient(TestUrl, http);

            var token1 = client.GetCancellationFromKey("default");
            var token2 = client.GetCancellationFromKey("2");
            var token3 = client.GetCancellationFromKey("3");

            client.Dispose();

            Assert.IsTrue(token1.IsCancellationRequested);
            Assert.IsTrue(token2.IsCancellationRequested);
            Assert.IsTrue(token3.IsCancellationRequested);
        }
    }
}
