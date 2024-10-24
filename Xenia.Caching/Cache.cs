using Byrone.Xenia.Utilities;

namespace Byrone.Xenia
{
	public static class Cache
	{
		public static System.ReadOnlySpan<byte> GetCacheHeaders(System.Span<byte> buffer, Cacheable cacheable)
		{
			const string dateTimeFormat = @"ddd\, dd MMM yyyy HH\:mm\:ss zzz";

			var builder = new StringBuilder(buffer);

			builder.AppendFormatted("Cache-Control: "u8);

			if (cacheable.Type == CacheType.NoCache)
			{
				// Most of these kitchen-sink headers are here for compatibility only.
				builder.AppendFormatted(
					"must-understand, no-store, no-cache, max-age=0, must-revalidate, proxy-revalidate"u8
				);

				return builder.Result;
			}

			var type = cacheable.Type == CacheType.Public ? "public"u8 : "private"u8;

			builder.AppendFormatted(type);
			builder.AppendFormatted(", must-revalidate, max-age="u8);
			builder.AppendFormatted(cacheable.Age);
			builder.AppendFormatted("\nETag: \""u8);
			builder.AppendFormatted(cacheable.ETag);
			builder.AppendFormatted((byte)'"');

			if (!cacheable.Vary.IsEmpty)
			{
				builder.AppendFormatted("\nVary: "u8);
				builder.AppendFormatted(cacheable.Vary);
			}

			// If the cacheable has a valid 'LastModified' value, the headers should contain this value AND the `Date` header
			if (cacheable.LastModified != default)
			{
				var now = System.DateTime.UtcNow;

				builder.AppendFormatted("\nDate: "u8);
				builder.AppendFormatted(now, dateTimeFormat);
				builder.AppendFormatted("\nLast-Modified: "u8);
				builder.AppendFormatted(cacheable.LastModified, dateTimeFormat);
			}

			return builder.Result;
		}
	}
}
