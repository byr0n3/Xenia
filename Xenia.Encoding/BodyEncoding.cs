using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace Byrone.Xenia
{
	public static class BodyEncoding
	{
		/// <summary>
		/// Based on the encoding <paramref name="method"/>, open a encoding stream that writes the encoded bytes
		/// based off <paramref name="data"/> to the given output <paramref name="outputStream"/>.
		/// </summary>
		/// <param name="outputStream">The <see cref="Stream"/> to output the encoded bytes to.</param>
		/// <param name="method">The encoding method to apply.</param>
		/// <param name="data">The data to encode.</param>
		/// <param name="level">The <see cref="CompressionLevel"/> to use.</param>
		/// <returns><see langword="true"/> if the data has been encoded, <see langword="false"/> otherwise.</returns>
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

			if (BodyEncoding.Is(method, "gzip"u8))
			{
				stream = new GZipStream(outputStream, level, true);
			}
			else if (BodyEncoding.Is(method, "br"u8))
			{
				stream = new BrotliStream(outputStream, level, true);
			}
			else if (BodyEncoding.Is(method, "deflate"u8))
			{
				stream = new DeflateStream(outputStream, level, true);
			}
			else
			{
				return false;
			}

			stream.Write(data);
			stream.Dispose();

			return true;
		}

		/// <summary>
		/// Based on the encoding <paramref name="method"/>, open a decoding stream that writes the decoded bytes
		/// based off <paramref name="data"/> to the given output <paramref name="outputStream"/>.
		/// </summary>
		/// <param name="dst">The destination to write the decoded bytes to.</param>
		/// <param name="data">The data to encode.</param>
		/// <param name="method">The encoding method to apply.</param>
		/// <param name="written">The amount of bytes written to the output stream.</param>
		/// <returns><see langword="true"/> if the data has been decoded, <see langword="false"/> otherwise.</returns>
		public static bool TryDecode(scoped System.Span<byte> dst,
									 scoped System.ReadOnlySpan<byte> data,
									 scoped System.ReadOnlySpan<byte> method,
									 out int written)
		{
			var inputStream = new RentedMemoryStream(data.Length);
			inputStream.Write(data);
			inputStream.Position = 0;

			Stream decodeStream;

			if (BodyEncoding.Is(method, "gzip"u8))
			{
				decodeStream = new GZipStream(inputStream, CompressionMode.Decompress, false);
			}
			else if (BodyEncoding.Is(method, "br"u8))
			{
				decodeStream = new BrotliStream(inputStream, CompressionMode.Decompress, false);
			}
			else if (BodyEncoding.Is(method, "deflate"u8))
			{
				decodeStream = new DeflateStream(inputStream, CompressionMode.Decompress, false);
			}
			else
			{
				written = default;
				return false;
			}

			written = decodeStream.Read(dst);
			decodeStream.Dispose();

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool Is(scoped System.ReadOnlySpan<byte> a, scoped System.ReadOnlySpan<byte> b) =>
			System.MemoryExtensions.SequenceEqual(a, b);
	}
}
