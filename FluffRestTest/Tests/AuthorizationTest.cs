using FluffRest.Client;
using FluffRestTest.Infra;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluffRestTest.Tests
{
    [TestClass]
    public class AuthorizationTest : BaseTest
    {
        [TestMethod]
        public async Task TestAddBasicAuthRequest()
        {
            // Arrange

            var url = $"{TestUrl}/auth";
            var httpClient = GetMockedHeaderClient(url, HttpMethod.Get, "Authorization", "Basic dGVzdDpwYXNzd29yZA==");
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);
            fluffClient = fluffClient.AddBasicAuth("test", "password");

            // Act
            await fluffClient.Get("auth").ExecAsync();
        }

        [TestMethod]
        public async Task TestAddBearerAuthRequest()
        {
            // Arrange

            var url = $"{TestUrl}/auth";
            var token = "token";
            var httpClient = GetMockedHeaderClient(url, HttpMethod.Get, "Authorization", $"Bearer {token}");
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);
            fluffClient = fluffClient.AddBearerAuth(token);

            // Act
            await fluffClient.Get("auth").ExecAsync();
        }

        [TestMethod]
        public async Task TestAddCustomAuthRequest()
        {
            // Arrange

            var url = $"{TestUrl}/auth";
            var scheme = "Test";
            var value = "Auth";
            var httpClient = GetMockedHeaderClient(url, HttpMethod.Get, "Authorization", $"{scheme} {value}");
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);
            fluffClient = fluffClient.AddAuth(scheme, value);

            // Act
            await fluffClient.Get("auth").ExecAsync();
        }
    }
}
