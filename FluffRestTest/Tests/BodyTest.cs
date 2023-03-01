using FluffRest.Client;
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
    public class BodyTest : BaseTest
    {
        [TestMethod]
        public async Task TestAddJsonBodyRequestAsync()
        {
            // Arrange

            var dto = GetBasicDto();
            var url = $"{TestUrl}/body";
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var httpClient = GetMockedBodyClient(url, HttpMethod.Post, json);
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);

            // Act
            await fluffClient.Post("body")
                .AddBody(dto)
                .ExecAsync();
        }
    }
}
