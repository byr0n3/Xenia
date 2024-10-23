using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace Byrone.Xenia
{
	public static class BodyEncoding
	{
		/// <summary>
		/// Based on the encoding <paramref name="method"/>, open a compression stream that writes to the given output <paramref name="outputStream"/>.
		/// </summary>
		/// <param name="outputStream">The <see cref="Stream"/> to output the encoded bytes to.</param>
		/// <param name="method">The encoding method to apply.</param>
		/// <param name="data">The data to encode.</param>
		/// <param name="level">The <see cref="CompressionLevel"/> to use.</param>
		/// <returns><see langword="true"/> if the data has been compressed, <see langword="false"/> otherwise.</returns>
		/// <remarks>
		/// If this function returns <see langword="true"/>, you have to add the <c>Content-Encoding:</c> header with the given method to the response,
		/// or the client <b>WON'T</b> be able to parse your response.
		/// </remarks>
		public static bool TryEncode(Stream outputStream,
									 scoped System.ReadOnlySpan<byte> method,
									 scoped System.ReadOnlySpan<byte> data,
									 CompressionLevel level = CompressionLevel.Optimal)
		{
			Stream stream;

			if (Is(method, "gzip"u8))
			{
				stream = new GZipStream(outputStream, level, true);
			}
			else if (Is(method, "br"u8))
			{
				stream = new BrotliStream(outputStream, level, true);
			}
			else if (Is(method, "deflate"u8))
			{
				stream = new DeflateStream(outputStream, level, true);
			}
			else
			{
				return false;
			}

			stream.Write(data);
			stream.Close();

			return true;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static bool Is(scoped System.ReadOnlySpan<byte> a, scoped System.ReadOnlySpan<byte> b) =>
				System.MemoryExtensions.SequenceEqual(a, b);
		}
	}
}
