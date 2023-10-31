using System.IO.Compression;
using System.Net.Sockets;
using Bytes = System.ReadOnlySpan<byte>;

namespace Byrone.Xenia.Helpers
{
	internal static class Compression
	{
		public static void GZip(NetworkStream stream, Bytes bytes)
		{
			using (var compressor = new GZipStream(stream, CompressionLevel.Optimal, true))
			{
				compressor.Write(bytes);
			}
		}

		public static void Deflate(NetworkStream stream, Bytes bytes)
		{
			using (var compressor = new DeflateStream(stream, CompressionLevel.Optimal, true))
			{
				compressor.Write(bytes);
			}
		}
	}
}
