using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FluffRest.Serializer
{
    public class JsonFluffSerializer : IFluffSerializer
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonFluffSerializer(JsonSerializerOptions jsonOption)
        {
            _jsonOptions = jsonOption;
        }

        public async Task<T> DeserializeAsync<T>(Stream value, CancellationToken cancellationToken)
        {
            T result = await JsonSerializer.DeserializeAsync<T>(value, _jsonOptions, cancellationToken: cancellationToken);
            return result;
        }

        public Task<string> SerializeAsync<T>(T value, CancellationToken cancellationToken)
        {
            Stream jsonStream = new MemoryStream();
            JsonSerializer.SerializeAsync(jsonStream, value, typeof(T), _jsonOptions, cancellationToken);

            using var reader = new StreamReader(jsonStream);
            return reader.ReadToEndAsync();
        }
    }
}
