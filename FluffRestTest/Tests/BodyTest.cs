using FluffRest.Client;
using FluffRest.Serializer;
using FluffRestTest.Infra;
using System;
using System.IO;
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
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestDuplicateBody()
        {
            var dto = GetBasicDto();
            var url = $"{TestUrl}/body";
            var request = new FluffRestClient(url, new HttpClient()).Get("error");
            request.AddBody(dto);
            request.AddBody(dto);
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

        public class TestDto
        {
            public DateTime Date { get; set; }
        }
    }
}
