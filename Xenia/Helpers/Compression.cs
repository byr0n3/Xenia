using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using Byrone.Xenia.Data;

namespace Byrone.Xenia.Helpers
{
	internal static class Compression
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Stream GetWriteStream(Stream stream,
											CompressionMethod compression,
											CompressionLevel level = CompressionLevel.Optimal)
		{
			return compression switch
			{
				CompressionMethod.GZip    => new GZipStream(stream, level, true),
				CompressionMethod.Deflate => new DeflateStream(stream, level, true),
				CompressionMethod.Brotli  => new BrotliStream(stream, level, true),
				_                         => stream,
			};
		}
	}
}
