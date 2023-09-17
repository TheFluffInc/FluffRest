using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace FluffRest.Compression
{
    public interface IFluffCompressor
    {
        /// <summary>
        /// Decribe which http header this compressor contaisn the logic for.
        /// </summary>
        string AcceptHeaderName { get; }

        /// <summary>
        /// Decompress input stream into byte array
        /// </summary>
        /// <param name="input">stream to be compressed</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<byte[]> DecompressAsync(Stream input, CancellationToken cancellationToken);
    }
}
