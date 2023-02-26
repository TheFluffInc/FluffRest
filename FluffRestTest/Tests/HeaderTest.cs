using FluffRest.Client;
using FluffRest.Exception;
using FluffRest.Settings;
using FluffRestTest.Dto;
using FluffRestTest.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FluffRestTest.Tests
{
    [TestClass]
    public class HeaderTest : BaseTest
    {
        [TestMethod]
        public async Task TestAddDefaultHeaderRequest()
        {
            // Arrange

            var url = $"{TestUrl}/header";
            var httpClient = GetMockedHeaderClient(url, HttpMethod.Get, "x-test", "test");
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);
            fluffClient = fluffClient.AddDefaultHeader("x-test", "test");

            // Act
            await fluffClient.Get("header").ExecAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(FluffDuplicateParameterException))]
        public void TestAddDuplicateDefaultHeaderThrows()
        {
            var url = $"{TestUrl}/header";
            var httpClient = GetMockedHeaderClient(url, HttpMethod.Get, "x-test", "test");
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);
            fluffClient = fluffClient.AddDefaultHeader("x-test", "test");
            fluffClient = fluffClient.AddDefaultHeader("x-test", "test");
        }

        [TestMethod]
        public async Task TestAddDuplicateDefaultHeaderReplaceAsync()
        {
            // Arrange

            var url = $"{TestUrl}/header";
            var httpClient = GetMockedHeaderClient(url, HttpMethod.Get, "x-test", "replaced");
            FluffClientSettings settings = new FluffClientSettings(duplicateHeaderHandling: FluffDuplicateHeaderHandling.Replace);
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient, settings);
            fluffClient = fluffClient.AddDefaultHeader("x-test", "test");
            fluffClient = fluffClient.AddDefaultHeader("x-test", "replaced");

            // Act
            await fluffClient.Get("header").ExecAsync();
        }

        [TestMethod]
        public async Task TestAddDuplicateDefaultHeaderIgnoreAsync()
        {
            // Arrange

            var url = $"{TestUrl}/header";
            var httpClient = GetMockedHeaderClient(url, HttpMethod.Get, "x-test", "test");
            FluffClientSettings settings = new FluffClientSettings(duplicateHeaderHandling: FluffDuplicateHeaderHandling.Ignore);
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient, settings);
            fluffClient = fluffClient.AddDefaultHeader("x-test", "test");
            fluffClient = fluffClient.AddDefaultHeader("x-test", "replaced");

            // Act
            await fluffClient.Get("header").ExecAsync();
        }

        [TestMethod]
        public async Task TestAddHeaderToRequest()
        {
            // Arrange

            var url = $"{TestUrl}/header";
            var httpClient = GetMockedHeaderClient(url, HttpMethod.Get, "x-test", "test");
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);

            // Act
            await fluffClient.Get("header").AddHeader("x-test", "test").ExecAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(FluffDuplicateParameterException))]
        public async Task TestAddDuplicateHeaderToRequestThrowsAsync()
        {
            // Arrange

            var url = $"{TestUrl}/header";
            var httpClient = GetMockedHeaderClient(url, HttpMethod.Get, "x-test", "test");
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);

            // Act
            await fluffClient.Get("header")
                .AddHeader("x-test", "test")
                .AddHeader("x-test", "test")
                .ExecAsync();
        }

        [TestMethod]
        public async Task TestAddDuplicateHeaderToRequestReplaceAsync()
        {
            // Arrange

            var url = $"{TestUrl}/header";
            var httpClient = GetMockedHeaderClient(url, HttpMethod.Get, "x-test", "replaced");
            FluffClientSettings settings = new FluffClientSettings(duplicateHeaderHandling: FluffDuplicateHeaderHandling.Replace);
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient, settings);

            // Act
            await fluffClient.Get("header")
                .AddHeader("x-test", "test")
                .AddHeader("x-test", "replaced")
                .ExecAsync();
        }

        [TestMethod]
        public async Task TestAddDuplicateHeaderToRequestIgnoreAsync()
        {
            // Arrange

            var url = $"{TestUrl}/header";
            var httpClient = GetMockedHeaderClient(url, HttpMethod.Get, "x-test", "test");
            FluffClientSettings settings = new FluffClientSettings(duplicateHeaderHandling: FluffDuplicateHeaderHandling.Ignore);
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient, settings);

            // Act
            await fluffClient.Get("header")
                .AddHeader("x-test", "test")
                .AddHeader("x-test", "replaced")
                .ExecAsync();
        }

        [TestMethod]
        public async Task TestJsonAcceptHeaderAsync()
        {
            // Arrange
            var dto = GetBasicDto();
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var url = $"{TestUrl}/header";
            var httpClient = GetMockedHeaderClient(url, HttpMethod.Get, "Accept", "application/json", json);
            FluffClientSettings settings = new FluffClientSettings(duplicateHeaderHandling: FluffDuplicateHeaderHandling.Ignore);
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient, settings);

            // Act
            await fluffClient.Get("header").ExecAsync<TestUserDto>();
        }

        [TestMethod]
        [ExpectedException(typeof(FluffDuplicateParameterException))]
        public async Task TestConflictingHeadersThrowsAsync()
        {
            // Arrange

            var url = $"{TestUrl}/header";
            var httpClient = GetMockedHeaderClient(url, HttpMethod.Get, "x-test", "test");
            FluffClientSettings settings = new FluffClientSettings(duplicateDefaultHeaderHandling: FluffDuplicateWithDefaultHeaderHandling.Throw);
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient, settings);
            fluffClient = fluffClient.AddDefaultHeader("x-test", "test");

            // Act
            await fluffClient.Get("header")
                .AddHeader("x-test", "test")
                .ExecAsync();
        }

        [TestMethod]
        public async Task TestConflictingHeadersReplaceAsync()
        {
            // Arrange

            var url = $"{TestUrl}/header";
            var httpClient = GetMockedHeaderClient(url, HttpMethod.Get, "x-test", "replaced");
            FluffClientSettings settings = new FluffClientSettings(duplicateDefaultHeaderHandling: FluffDuplicateWithDefaultHeaderHandling.Replace);
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient, settings);
            fluffClient = fluffClient.AddDefaultHeader("x-test", "test");

            // Act
            await fluffClient.Get("header")
                .AddHeader("x-test", "replaced")
                .ExecAsync();
        }

        [TestMethod]
        public async Task TestConflictingHeadersIgnoreAsync()
        {
            // Arrange

            var url = $"{TestUrl}/header";
            var httpClient = GetMockedHeaderClient(url, HttpMethod.Get, "x-test", "test");
            FluffClientSettings settings = new FluffClientSettings(duplicateDefaultHeaderHandling: FluffDuplicateWithDefaultHeaderHandling.Ignore);
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient, settings);
            fluffClient = fluffClient.AddDefaultHeader("x-test", "test");

            // Act
            await fluffClient.Get("header")
                .AddHeader("x-test", "test")
                .ExecAsync();
        }
    }
}
