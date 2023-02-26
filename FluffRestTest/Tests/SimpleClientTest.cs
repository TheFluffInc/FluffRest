using FluffRest.Client;
using FluffRest.Exception;
using FluffRestTest.Dto;
using FluffRestTest.Infra;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluffRestTest.Tests
{
    [TestClass]
    public class SimpleClientTest : BaseTest
    {
        [TestMethod]
        public async Task TestExcecuteBasicGetRequest()
        {
            // Arrange

            var dto = GetBasicDto();
            var url = $"{TestUrl}/simple";
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var httpClient = GetMockedClient(url, JsonContentType, json, HttpMethod.Get);
            var fluffClient = new FluffRestClient(TestUrl, httpClient);

            // Act
            var result = await fluffClient.Get("simple").ExecAsync<TestUserDto>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, dto.Id);
            Assert.AreEqual(result.Name, dto.Name);
        }

        [TestMethod]
        public async Task TestExcecuteBasicPostRequest()
        {
            // Arrange

            var dto = GetBasicDto();
            var url = $"{TestUrl}/simple";
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var httpClient = GetMockedClient(url, JsonContentType, json, HttpMethod.Post);
            var fluffClient = new FluffRestClient(TestUrl, httpClient);

            // Act
            var result = await fluffClient.Post("simple").ExecAsync<TestUserDto>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, dto.Id);
            Assert.AreEqual(result.Name, dto.Name);
        }

        [TestMethod]
        public async Task TestExcecuteBasicPatchRequest()
        {
            // Arrange

            var dto = GetBasicDto();
            var url = $"{TestUrl}/simple";
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var httpClient = GetMockedClient(url, JsonContentType, json, HttpMethod.Patch);
            var fluffClient = new FluffRestClient(TestUrl, httpClient);

            // Act
            var result = await fluffClient.Patch("simple").ExecAsync<TestUserDto>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, dto.Id);
            Assert.AreEqual(result.Name, dto.Name);
        }

        [TestMethod]
        public async Task TestExcecuteBasicPutRequest()
        {
            // Arrange

            var dto = GetBasicDto();
            var url = $"{TestUrl}/simple";
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var httpClient = GetMockedClient(url, JsonContentType, json, HttpMethod.Put);
            var fluffClient = new FluffRestClient(TestUrl, httpClient);

            // Act
            var result = await fluffClient.Put("simple").ExecAsync<TestUserDto>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, dto.Id);
            Assert.AreEqual(result.Name, dto.Name);
        }

        [TestMethod]
        public async Task TestExcecuteBasicDeleteRequest()
        {
            // Arrange

            var dto = GetBasicDto();
            var url = $"{TestUrl}/simple";
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var httpClient = GetMockedClient(url, JsonContentType, json, HttpMethod.Delete);
            var fluffClient = new FluffRestClient(TestUrl, httpClient);

            // Act
            var result = await fluffClient.Delete("simple").ExecAsync<TestUserDto>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, dto.Id);
            Assert.AreEqual(result.Name, dto.Name);
        }

        [TestMethod]
        [DynamicData(nameof(GetHttpMethods), DynamicDataSourceType.Method)]
        public async Task TestExcecuteBasicCustomRequest(HttpMethod method)
        {
            // Arrange

            var dto = GetBasicDto();
            var url = $"{TestUrl}/simple";
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var httpClient = GetMockedClient(url, JsonContentType, json, method);
            var fluffClient = new FluffRestClient(TestUrl, httpClient);

            // Act
            var result = await fluffClient.Request(method, "simple").ExecAsync<TestUserDto>();

            // Assert
            Assert.IsNotNull(result); 
            Assert.AreEqual(result.Id, dto.Id);
            Assert.AreEqual(result.Name, dto.Name);
        }

        #region Other

        public static IEnumerable<object[]> GetHttpMethods()
        {
            yield return new object[] { HttpMethod.Get };
            yield return new object[] { HttpMethod.Post };
            yield return new object[] { HttpMethod.Patch };
            yield return new object[] { HttpMethod.Put };
            yield return new object[] { HttpMethod.Delete };
            yield return new object[] { HttpMethod.Head };
            yield return new object[] { HttpMethod.Options };
        }

        #endregion
    }
}
