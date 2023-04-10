using System;
using System.Collections;
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

        public async Task<string> SerializeAsync<T>(T value, CancellationToken cancellationToken)
        {
            MemoryStream jsonStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(jsonStream, value, typeof(T), _jsonOptions, cancellationToken);
            return Encoding.UTF8.GetString(jsonStream.ToArray());
        }
    }
}
