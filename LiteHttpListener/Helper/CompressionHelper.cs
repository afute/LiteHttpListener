using System.IO.Compression;

namespace LiteHttpListener.Helper;

public static class CompressionHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<byte[]> GZipCompressAsync(byte[] data, CancellationToken? token = null)
    {
        var token1 = token ?? CancellationToken.None;
        const CompressionMode mode = CompressionMode.Compress;
        await using var memoryStream = new MemoryStream();
        await using (var gzipStream = new GZipStream(memoryStream, mode, true))
        {
            await gzipStream.WriteAsync(data, token1);
        }
        return memoryStream.ToArray();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<byte[]> GZipDecompressAsync(byte[] data, CancellationToken? token = null)
    {
        var token1 = token ?? CancellationToken.None;
        const CompressionMode mode = CompressionMode.Decompress;
        await using var compressedStream = new MemoryStream(data);
        await using var gzipStream = new GZipStream(compressedStream, mode);
        await using var resultStream = new MemoryStream();
        await gzipStream.CopyToAsync(resultStream, token1);
        return resultStream.ToArray();
    }
}
