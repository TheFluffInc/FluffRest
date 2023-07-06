using System.Net;

namespace FluffRest.Request.Advanced
{
    public sealed class FluffAdvancedResponse<T>
    {
        /// <summary>
        /// Deserialized content from response.
        /// </summary>
        public T Content { get; private set; }

        /// <summary>
        /// Status code from response.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        internal FluffAdvancedResponse(T content, HttpStatusCode statusCode)
        {
            Content = content;
            StatusCode = statusCode;
        }
    }
}
