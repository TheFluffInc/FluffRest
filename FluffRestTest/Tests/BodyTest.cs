using FluffRest.Client;
using FluffRest.Serializer;
using FluffRestTest.Infra;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluffRestTest.Tests
{
    [TestClass]
    public class BodyTest : BaseTest
    {
        [TestMethod]
        public async Task TestAddJsonBodyRequestAsync()
        {
            // Arrange

            var dto = GetBasicDto();
            var url = $"{TestUrl}/body";
            var serializer = new JsonFluffSerializer(new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
            var json = await serializer.SerializeAsync(dto, CancellationToken.None);

            var httpClient = GetMockedBodyClient(url, HttpMethod.Post, json);
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient, serializer: serializer);

            // Act
            await fluffClient.Post("body")
                .AddBody(dto)
                .ExecAsync();
        }

        [TestMethod]
        public async Task TestAsync()
        {
            string json = "{\"date\": \"2023-05-13T21:15:01.000Z\"}";
            var serializer = new JsonFluffSerializer(new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
            TestDto result = await serializer.DeserializeAsync<TestDto>(new MemoryStream(Encoding.UTF8.GetBytes(json)), CancellationToken.None);
            Console.WriteLine(result.Date.ToString());
        }

        [TestMethod]
        public void TestDuplicateBody()
        {
            try
            {
                var dto = GetBasicDto();
                var url = $"{TestUrl}/body";
                var request = new FluffRestClient(url, new HttpClient()).Get("error");
                request.AddBody(dto);
                request.AddBody(dto);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType<InvalidOperationException>(ex);
            }
        }

        [TestMethod]
        public async Task TestAddRawBodyRequestAsync()
        {
            // Arrange

            var url = $"{TestUrl}/body";
            var body = "test13456";
            var contentType = "text/plain";
            var httpClient = GetMockedBodyClientWithContentType(url, HttpMethod.Post, body, contentType);
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);

            // Act
            await fluffClient.Post("body")
                .AddBody(body, contentType)
                .ExecAsync();
        }

        [TestMethod]
        public async Task TestEmptyResponseShouldReturnDefaultAsync()
        {
            // Arrange

            var url = $"{TestUrl}/empty";
            HttpResponseMessage response = new HttpResponseMessage()
            {
                Content = new StringContent(string.Empty),
                StatusCode = HttpStatusCode.NoContent
            };

            var httpClient = GetMockedClient(url, response, HttpMethod.Get);
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);

            // Act
            var result = await fluffClient.Get("empty").ExecAsync<TestDto>();

            // Assert
            Assert.AreEqual(default, result);
        }

        [TestMethod]
        public async Task TestAdvancedEmptyResponseShouldReturnDefaultAsync()
        {
            // Arrange

            var url = $"{TestUrl}/empty";
            HttpResponseMessage response = new HttpResponseMessage()
            {
                Content = new StringContent(string.Empty),
                StatusCode = HttpStatusCode.NoContent
            };

            var httpClient = GetMockedClient(url, response, HttpMethod.Get);
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);

            // Act
            var result = await fluffClient.Get("empty").ExecAdvancedAsync<TestDto>();

            // Assert
            Assert.AreEqual(default, result.Content);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }
        
        [TestMethod]
        public async Task TestStreamAsync()
        {
            // Arrange

            string content = "{\"date\": \"2023-05-13T21:15:01.000Z\"}";
            var url = $"{TestUrl}/stream";
            HttpResponseMessage response = new HttpResponseMessage()
            {
                Content = new StringContent(content),
                StatusCode = HttpStatusCode.Created
            };

            var httpClient = GetMockedClient(url, response, HttpMethod.Get);
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);

            // Act
            await using var result = await fluffClient.Get("stream").ExecStreamAsync();
            
            // Assert
            
            using var reader = new StreamReader(result.ContentStream);
            var readContent = await reader.ReadToEndAsync();
            
            Assert.AreEqual(content, readContent);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        public class TestDto
        {
            public DateTime Date { get; set; }
        }
    }
}
