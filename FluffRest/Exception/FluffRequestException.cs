using FluffRest.Serializer;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace FluffRest.Exception
{
    public class FluffRequestException : System.Exception
    {
        private readonly IFluffSerializer _serializer;
        public HttpStatusCode StatusCode { get; private set; }
        public string Content { get; private set; }

        public FluffRequestException(string message, string content, HttpStatusCode httpStatusCode, IFluffSerializer serializer, System.Exception inner)
            : base (message, inner)
        {
            _serializer = serializer;
            Content = content;
            StatusCode = httpStatusCode;
        }

        public Task<T> DeserializeAsync<T>(CancellationToken cancellationToken = default)
        {
            byte[] contentBytes = Encoding.UTF8.GetBytes(Content);
            MemoryStream stream = new MemoryStream(contentBytes);
            var jsonWebOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            return _serializer.DeserializeAsync<T>(stream, cancellationToken);
        }
    }
}
