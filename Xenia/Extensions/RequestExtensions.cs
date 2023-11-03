using Byrone.Xenia.Data;
using Byrone.Xenia.Helpers;
using JetBrains.Annotations;

namespace Byrone.Xenia.Extensions
{
	[PublicAPI]
	public static class RequestExtensions
	{
		public static bool TryGetHeader(in this Request @this, System.ReadOnlySpan<byte> key, out KeyValue header)
		{
			foreach (var head in @this.Headers)
			{
				if (!head.Key.Equals(key))
				{
					continue;
				}

				header = head;
				return true;
			}

			header = default;
			return false;
		}

		public static CompressionMethod GetCompressionMethod(in this Request @this)
		{
			var supported = @this.SupportedCompression;

			if (RequestExtensions.TryGetHeader(in @this, Headers.AcceptEncoding, out var acceptEncoding) &&
				ServerHelpers.TryGetValidCompressionMode(acceptEncoding.Value, supported, out var compression))
			{
				return compression;
			}

			return CompressionMethod.None;
		}

		public static System.ReadOnlySpan<byte> GetCompressionMethodHeader(in this Request @this)
		{
			var method = RequestExtensions.GetCompressionMethod(in @this);

			return method switch
			{
				CompressionMethod.GZip    => "gzip"u8,
				CompressionMethod.Deflate => "deflate"u8,
				CompressionMethod.Brotli  => "br"u8,
				_                         => default,
			};
		}
	}
}
