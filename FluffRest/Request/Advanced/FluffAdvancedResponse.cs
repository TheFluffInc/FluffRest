using System;
using FluffRest.Serializer;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

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

    public sealed class FluffAdvancedResponse
    {
        private readonly IFluffSerializer _serializer;

        /// <summary>
        /// Raw content.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Status code from response.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        internal FluffAdvancedResponse(string content, HttpStatusCode statusCode, IFluffSerializer serializer)
        {
            Content = content;
            StatusCode = statusCode;
            _serializer = serializer;
        }

        public Task<T> DeserializeAsync<T>(CancellationToken cancellationToken = default)
        {
            byte[] contentBytes = Encoding.UTF8.GetBytes(Content);
            MemoryStream stream = new MemoryStream(contentBytes);
            return _serializer.DeserializeAsync<T>(stream, cancellationToken);
        }
    }

    public sealed class FluffStreamAdvancedResponse : IAsyncDisposable
    {
        /// <summary>
        /// The stream of the http content.
        /// </summary>
        public Stream ContentStream { get; private set; }
        
        /// <summary>
        /// Status code from response.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        public FluffStreamAdvancedResponse(Stream contentStream, HttpStatusCode statusCode)
        {
            ContentStream = contentStream;
            StatusCode = statusCode;
        }
        
        public async ValueTask DisposeAsync()
        {
            if (ContentStream != null)
            {
                await ContentStream.DisposeAsync();
            }
        }
    }
}
