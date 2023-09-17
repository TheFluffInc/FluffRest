using FluffRest.Client;
using FluffRest.Compression;
using FluffRest.Serializer;
using FluffRestTest.Infra;
using FluffRestTest.Mocks;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace FluffRestTest.Tests
{
    [TestClass]
    public class CompressionTests : BaseTest
    {
        [TestMethod]
        public async Task TestAcceptEncodingHeaderIsSetAsync()
        {
            // Arrange

            var url = $"{TestUrl}/encoding";
            var httpClient = GetMockedClient(url, HttpMethod.Get);
            var listener = new MockListener();
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);
            fluffClient = fluffClient.RegisterListener(listener);
            fluffClient = fluffClient.RegisterCompression(new BrotliFluffCompressor());

            // Act
            await fluffClient.Get("encoding").ExecAsync();

            // Assert
            Assert.AreEqual("br", listener.LastRequest.Headers.AcceptEncoding?.FirstOrDefault().Value);
        }

        [TestMethod]
        public async Task TestAcceptEncodingHeaderIsNotSetAsync()
        {
            // Arrange

            var url = $"{TestUrl}/encoding";
            var httpClient = GetMockedClient(url, HttpMethod.Get);
            var listener = new MockListener();
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);
            fluffClient = fluffClient.RegisterListener(listener);

            // Act
            await fluffClient.Get("encoding").ExecAsync();

            // Assert
            Assert.AreEqual(null, listener.LastRequest.Headers.AcceptEncoding?.FirstOrDefault()?.Value);
        }

        [TestMethod]
        public async Task TestDecodeResponseAsync()
        {
            // Arrange

            var url = $"{TestUrl}/encoding";
            var dto = GetBasicDto();
            var serializer = new JsonFluffSerializer(new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
            var json = await serializer.SerializeAsync(dto, CancellationToken.None);

            byte[] compressedBody;

            using (MemoryStream input = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            using (MemoryStream result = new MemoryStream(1048576))
            using (BrotliStream brotli = new BrotliStream(result, CompressionMode.Compress))
            {
                await input.CopyToAsync(brotli);
                await brotli.FlushAsync();
                compressedBody = result.ToArray();
            }

            var httpClient = GetMockedBodyClient(url, HttpMethod.Get, compressedBody, "br");
            var listener = new MockListener();
            IFluffRestClient fluffClient = new FluffRestClient(TestUrl, httpClient);
            fluffClient = fluffClient.RegisterListener(listener);
            fluffClient = fluffClient.RegisterCompression(new BrotliFluffCompressor());

            // Act
            var requestResult = await fluffClient.Get("encoding").ExecStringAsync();

            // Assert
            Assert.AreEqual(json, requestResult);
        }
    }
}
