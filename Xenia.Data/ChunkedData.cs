using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Byrone.Xenia.Internal;
using Byrone.Xenia.Internal.Extensions;

namespace Byrone.Xenia
{
	public static class ChunkedData
	{
		/// <summary>
		/// Get the total size of the chunked data.
		/// </summary>
		/// <param name="src">The chunked data.</param>
		/// <returns>The total size of the chunked data.</returns>
		/// <remarks>
		/// <para>This function is useful to identify how much data should be allocated/rented to parse the data.</para>
		/// <para>This function loops over the entire data to sum all the chunk sizes, and thus can result in a performance penalty.</para>
		/// </remarks>
		public static int GetSize(scoped System.ReadOnlySpan<byte> src)
		{
			var enumerator = new SpanSplitEnumerator(src, Characters.HttpSeparator);

			var moved = enumerator.MoveNext();
			Debug.Assert(moved);

			var chunkLength = ChunkedData.GetChunkLength(enumerator.Current);
			var length = chunkLength;

			while (chunkLength > 0)
			{
				// Skip over chunk content
				moved = enumerator.MoveNext();
				Debug.Assert(moved);

				moved = enumerator.MoveNext();
				Debug.Assert(moved);

				chunkLength = ChunkedData.GetChunkLength(enumerator.Current);
				length += chunkLength;
			}

			return length;
		}

		/// <summary>
		/// Parses the given <paramref name="src"/> input as chunked data and puts the glued together parts into <paramref name="dst"/>.
		/// </summary>
		/// <param name="src">The chunked data.</param>
		/// <param name="dst">Destination where the chunked parts should be copied to.</param>
		/// <returns>Amount of written bytes.</returns>
		public static int Parse(scoped System.ReadOnlySpan<byte> src, System.Span<byte> dst)
		{
			var written = 0;

			var enumerator = new SpanSplitEnumerator(src, Characters.HttpSeparator);

			// Move to the first slice (chunk length)
			var moved = enumerator.MoveNext();
			Debug.Assert(moved);

			var chunkLength = ChunkedData.GetChunkLength(enumerator.Current);

			while (chunkLength > 0)
			{
				// Move to the next slice (chunk content)
				moved = enumerator.MoveNext();
				Debug.Assert(moved);

				var content = enumerator.Current.SliceUnsafe(0, chunkLength);

				if (content.TryCopyTo(dst.SliceUnsafe(written)))
				{
					written += chunkLength;
				}

				// Move to the next slice (either next chunk size, or trailer data)
				moved = enumerator.MoveNext();
				Debug.Assert(moved);

				// @todo If not hex-digit, skip

				// Skip empty trailers
				while (enumerator.Current.IsEmpty)
				{
					enumerator.MoveNext();
				}

				// If we reached the end of the content, an empty chunk or trailing data, the loop will exit
				chunkLength = ChunkedData.GetChunkLength(enumerator.Current);
			}

			return written;
		}

		/// <summary>
		/// Checks if the given <paramref name="request"/> has the <c>Transfer-Encoding: chunked</c> header.
		/// </summary>
		/// <param name="request">The incoming request.</param>
		/// <returns><see langword="true"/> if the request has the header, <see langword="false"/> otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasChunkedBody(in Request request) =>
			request.TryGetHeader("Transfer-Encoding"u8, out var transferEncoding) &&
			ChunkedData.IsChunkedEncoding(transferEncoding);

		/// <summary>
		/// Gets the encoding the chunked body uses, if applicable.
		/// </summary>
		/// <param name="request">The incoming request.</param>
		/// <returns>The encoding used for the chunked body if any, <see langword="default"/> otherwise.</returns>
		public static System.ReadOnlySpan<byte> GetChunkEncoding(in Request request)
		{
			if (!request.TryGetHeader("Transfer-Encoding"u8, out var transferEncoding))
			{
				return default;
			}

			// HTML/1.1 spec declares the format of `Transfer-Encoding` to be, for example:
			// - 'chunked'
			// - 'gzip, chunked'
			foreach (var part in new SpanSplitEnumerator(transferEncoding, ", "u8))
			{
				if (!ChunkedData.IsChunkedEncoding(part))
				{
					return part;
				}
			}

			return default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetChunkLength(scoped System.ReadOnlySpan<byte> data) =>
			int.TryParse(data, NumberStyles.AllowHexSpecifier, null, out var result) ? result : default;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsChunkedEncoding(scoped System.ReadOnlySpan<byte> value) =>
			// Header value could be `chunked` but also something like `gzip, chunked`.
			System.MemoryExtensions.IndexOf(value, "chunked"u8) != -1;
	}
}
