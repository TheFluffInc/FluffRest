using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FluffRest.Serializer
{
    public interface IFluffSerializer
    {
        Task<string> SerializeAsync<T>(T value, CancellationToken cancellationToken);
        Task<T> DeserializeAsync<T>(Stream value, CancellationToken cancellationToken);
    }
}
