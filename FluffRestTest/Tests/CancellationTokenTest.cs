using FluffRest.Client;
using FluffRestTest.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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
        public void TestCancellationTokenProvided()
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
    }
}
