using FluffRestTest.Dto;
using RichardSzalay.MockHttp;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluffRestTest.Infra
{
    public class BaseTest
    {
        protected const string TestUrl = "http://localhost/test";
        protected const string JsonContentType = "application/json";

        public HttpClient GetMockedClient(string url, string contentType, string response, HttpMethod method)
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(method, url).Respond(contentType, response);
            return mockHttp.ToHttpClient();
        }

        public HttpClient GetMockedClient(string url, HttpResponseMessage response, HttpMethod method)
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(method, url).Respond(req => response);
            return mockHttp.ToHttpClient();
        }

        public HttpClient GetMockedClient(string url, HttpMethod method)
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(method, url).Respond(req => new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK
            });
            return mockHttp.ToHttpClient();
        }

        public HttpClient GetMockedHeaderClient(string url, HttpMethod method, string name, string value, string content = null)
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(method, url).Respond(req => GenerateResponseIfHeaderExists(req, name, value, content));
            return mockHttp.ToHttpClient();
        }

        public HttpClient GetMockedBodyClient(string url, HttpMethod method, string body)
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(method, url).Respond(req => GenerateResponseIfBodyExistsAsync(req, body));
            return mockHttp.ToHttpClient();
        }

        public HttpClient GetMockedBodyClientWithContentType(string url, HttpMethod method, string body, string contentType = null)
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(method, url).Respond(req => GenerateResponseIfBodyExistsAsync(req, body, contentType));
            return mockHttp.ToHttpClient();
        }

        public TestUserDto GetBasicDto()
        {
            return new TestUserDto()
            {
                Id = 1,
                Name = "Test",
                Date = DateTime.UtcNow
            };
        }

        private HttpResponseMessage GenerateResponseIfHeaderExists(HttpRequestMessage req, string headerName, string headerValue, string content = null)
        {
            if (req.Headers.Any(x => x.Key == headerName && x.Value.Contains(headerValue)))
            {
                if (string.IsNullOrEmpty(content))
                {
                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                }
                else
                {
                    var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                    response.Content = new StringContent(content);
                    return response;
                }
            }
            else
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            }
        }

        private async Task<HttpResponseMessage> GenerateResponseIfBodyExistsAsync(HttpRequestMessage req, string body, string contentType = null)
        {
            var receivedBody = await req.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(contentType) && !req.Content.Headers.ContentType.MediaType.Equals(contentType))
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.UnsupportedMediaType);
            }

            if (receivedBody == body)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            else
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}
