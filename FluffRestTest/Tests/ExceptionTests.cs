using FluffRest.Client;
using FluffRest.Exception;
using FluffRest.Settings;
using FluffRestTest.Dto;
using FluffRestTest.Infra;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FluffRestTest.Tests
{
    [TestClass]
    public class ExceptionTests : BaseTest
    {
        [TestMethod]
        public async Task TestErrorContentForwardedWithJson()
        {
            // Arrange

            var dto = GetBasicDto();
            var url = $"{TestUrl}/error";
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            mockResponse.Content = new StringContent(json);
            var httpClient = GetMockedClient(url, mockResponse, HttpMethod.Get);
            var fluffClient = new FluffRestClient(TestUrl, httpClient);

            
            try
            {
                // Act

                var result = await fluffClient.Get("error").ExecAsync<TestUserDto>();
            }
            catch (FluffRequestException ex)
            {
                Assert.IsNotNull(ex.Content);

                var jsonResult = JsonConvert.DeserializeObject<TestUserDto>(ex.Content);

                // Assert
                Assert.IsNotNull(jsonResult);
                Assert.AreEqual(jsonResult.Id, dto.Id);
                Assert.AreEqual(jsonResult.Name, dto.Name);
            }
        }

        [TestMethod]
        public async Task TestErrorContentForwardedWithoutJson()
        {
            // Arrange

            var dto = GetBasicDto();
            var url = $"{TestUrl}/error";
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            mockResponse.Content = new StringContent(json);
            var httpClient = GetMockedClient(url, mockResponse, HttpMethod.Get);
            var fluffClient = new FluffRestClient(TestUrl, httpClient);


            try
            {
                // Act

                await fluffClient.Get("error").ExecAsync();
            }
            catch (FluffRequestException ex)
            {
                Assert.IsNotNull(ex.Content);

                var jsonResult = JsonConvert.DeserializeObject<TestUserDto>(ex.Content);

                // Assert
                Assert.IsNotNull(jsonResult);
                Assert.AreEqual(jsonResult.Id, dto.Id);
                Assert.AreEqual(jsonResult.Name, dto.Name);
            }
        }

        [TestMethod]
        public async Task TestExcecuteNoThrowOnUncesfullRequest()
        {
            // Arrange

            var dto = GetBasicDto();
            var url = $"{TestUrl}/error";
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            mockResponse.Content = new StringContent(json);
            var httpClient = GetMockedClient(url, mockResponse, HttpMethod.Get);
            var settings = new FluffClientSettings(ensureSuccessCode: false);
            var fluffClient = new FluffRestClient(TestUrl, httpClient, settings);

            // Act
            var result = await fluffClient.Get("error").ExecAsync<TestUserDto>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, dto.Id);
            Assert.AreEqual(result.Name, dto.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestProvideNullHttpClient()
        {
            _ = new FluffRestClient(TestUrl, null);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestProvideEmptyUrl()
        {
            _ = new FluffRestClient(string.Empty, new HttpClient());
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestProvideNullUrl()
        {
            _ = new FluffRestClient(null, new HttpClient());
        }
    }
}
