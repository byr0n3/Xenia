using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Byrone.Xenia.Data;

namespace Byrone.Xenia.Helpers
{
	internal static class Compression
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Stream GetWriteStream(Socket client,
											CompressionMethod compression,
											CompressionLevel level = CompressionLevel.Optimal)
		{
			var stream = new NetworkStream(client);

			return compression switch
			{
				CompressionMethod.GZip    => new GZipStream(stream, level, false),
				CompressionMethod.Deflate => new DeflateStream(stream, level, false),
				CompressionMethod.Brotli  => new BrotliStream(stream, level, false),
				_                         => stream,
			};
		}
	}
}
