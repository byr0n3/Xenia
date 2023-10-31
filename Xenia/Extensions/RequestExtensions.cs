using System.Runtime.CompilerServices;
using Byrone.Xenia.Data;
using Byrone.Xenia.Helpers;
using JetBrains.Annotations;

namespace Byrone.Xenia.Extensions
{
	[PublicAPI]
	public static class RequestExtensions
	{
		public static bool TryGetHeader(in this Request @this, System.ReadOnlySpan<byte> key, out RequestHeader header)
		{
			foreach (var head in @this.Headers)
			{
				if (!System.MemoryExtensions.SequenceEqual(head.Key, key))
				{
					continue;
				}

				header = head;
				return true;
			}

			header = default;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CompressionMethod GetCompressionMethod(in this Request @this)
		{
			var supported = @this.SupportedCompression;

			if (@this.TryGetHeader(Headers.AcceptEncoding, out var acceptEncoding) &&
				ServerHelpers.TryGetValidCompressionMode(acceptEncoding.Value, supported, out var compression))
			{
				return compression;
			}

			return CompressionMethod.None;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static System.ReadOnlySpan<byte> GetCompressionMethodHeader(in this Request @this)
		{
			var method = RequestExtensions.GetCompressionMethod(in @this);

			return method switch
			{
				CompressionMethod.GZip    => "gzip"u8,
				CompressionMethod.Deflate => "deflate"u8,
				CompressionMethod.Brotli  => "br"u8,
				_                         => System.ReadOnlySpan<byte>.Empty,
			};
		}
	}
}
