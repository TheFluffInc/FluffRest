using FluffRest.Client;
using FluffRestTest.Infra;
using FluffRestTest.Mocks;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluffRestTest.Tests
{
    [TestClass]
    public class ListenerTest : BaseTest
    {
        [TestMethod]
        public async Task TestListenerIsCalledBeforeAndAfterAsync()
        {
            // Arrange

            var url = $"{TestUrl}/listener";
            var httpClient = GetMockedClient(url, HttpMethod.Get);
            var listener = new MockListener();
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);
            fluffClient = fluffClient.RegisterListener(listener);

            // Act
            await fluffClient.Get("listener").ExecAsync();

            // Assert
            Assert.IsTrue(listener.IsRequestSentCalled);
            Assert.IsTrue(listener.IsAfterRequestCalled);
            Assert.IsFalse(listener.IsRequestFailedCalled);
        }

        [TestMethod]
        public async Task TestListenerIsCalledOnFailAsync()
        {
            // Arrange

            var url = $"{TestUrl}/listener";
            var httpClient = GetMockedClient(url, HttpMethod.Post);
            var listener = new MockListener();
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);
            fluffClient = fluffClient.RegisterListener(listener);

            // Act
            try
            {
                await fluffClient.Get("listener").ExecAsync();
            }
            catch
            {
            }

            // Assert
            Assert.IsTrue(listener.IsRequestSentCalled);
            Assert.IsFalse(listener.IsAfterRequestCalled);
            Assert.IsTrue(listener.IsRequestFailedCalled);
        }

        [TestMethod]
        public async Task TestBeforeListenerEditRequestAsync()
        {
            // Arrange

            var url = $"{TestUrl}/listener";
            var httpClient = GetMockedHeaderClient(url, HttpMethod.Get, "x-test-listner", "true");
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);
            fluffClient = fluffClient.RegisterListener(new MockListener());

            // Act
            await fluffClient.Get("listener").ExecAsync();
        }
    }
}
