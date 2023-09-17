using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluffRest.Compression
{
    public class BrotliFluffCompressor : IFluffCompressor
    {
        public string AcceptHeaderName => "br";

        public async Task<byte[]> DecompressAsync(Stream input, CancellationToken cancellationToken)
        {
            using (MemoryStream result = new MemoryStream())
            using (BrotliStream brotli = new BrotliStream(input, CompressionMode.Decompress))
            {
                await brotli.CopyToAsync(result);
                return result.ToArray();
            }
        }
    }
}
