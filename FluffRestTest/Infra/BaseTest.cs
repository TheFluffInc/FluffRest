using FluffRestTest.Dto;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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

        public TestUserDto GetBasicDto()
        {
            return new TestUserDto()
            {
                Id = 1,
                Name = "Test",
            };
        }
    }
}
