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
    public class QueryParametersTest : BaseTest
    {
        [TestMethod]
        public async Task BasicParameters()
        {
            // Arrange

            var dto = GetBasicDto();
            var url = $"{TestUrl}/simple?id=1&name=Test&short=2&long=3";
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var httpClient = GetMockedClient(url, JsonContentType, json, HttpMethod.Get);
            var fluffClient = new FluffRestClient(TestUrl, httpClient);

            // Act
            var result = await fluffClient.Get("simple")
                .AddQueryParameter("id", 1)
                .AddQueryParameter("name", "Test")
                .AddQueryParameter("short", (short)2)
                .AddQueryParameter("long", (long)3)
                .ExecAsync<TestUserDto>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, dto.Id);
            Assert.AreEqual(result.Name, dto.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(FluffDuplicateParameterException))]
        public async Task DuplicateParameterThrow()
        {
            // Arrange

            var dto = GetBasicDto();
            var url = $"{TestUrl}/simple?id=1&name=Test";
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var httpClient = GetMockedClient(url, JsonContentType, json, HttpMethod.Get);
            var fluffClient = new FluffRestClient(TestUrl, httpClient);

            // Act
            var result = await fluffClient.Get("simple")
                .AddQueryParameter("id", 1)
                .AddQueryParameter("name", "Test")
                .AddQueryParameter("name", "Test")
                .ExecAsync<TestUserDto>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, dto.Id);
            Assert.AreEqual(result.Name, dto.Name);
        }

        [TestMethod]
        public async Task DuplicateParameterIgnore()
        {
            // Arrange

            var dto = GetBasicDto();
            var url = $"{TestUrl}/simple?id=1&name=Test";
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var httpClient = GetMockedClient(url, JsonContentType, json, HttpMethod.Get);
            var options = new FluffClientSettings(FluffDuplicateParameterKeyHandling.Ignore);
            var fluffClient = new FluffRestClient(TestUrl, httpClient, options);

            // Act
            var result = await fluffClient.Get("simple")
                .AddQueryParameter("id", 1)
                .AddQueryParameter("name", "Test")
                .AddQueryParameter("name", "Test")
                .ExecAsync<TestUserDto>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, dto.Id);
            Assert.AreEqual(result.Name, dto.Name);
        }

        [TestMethod]
        public async Task DuplicateParameterReplace()
        {
            // Arrange

            var dto = GetBasicDto();
            var url = $"{TestUrl}/simple?id=1&name=Test";
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var httpClient = GetMockedClient(url, JsonContentType, json, HttpMethod.Get);
            var options = new FluffClientSettings(FluffDuplicateParameterKeyHandling.Replace);
            var fluffClient = new FluffRestClient(TestUrl, httpClient, options);

            // Act
            var result = await fluffClient.Get("simple")
                .AddQueryParameter("id", 1)
                .AddQueryParameter("name", "Baba")
                .AddQueryParameter("name", "Test")
                .ExecAsync<TestUserDto>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, dto.Id);
            Assert.AreEqual(result.Name, dto.Name);
        }
    }
}
