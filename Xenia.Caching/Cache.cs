using System.Buffers.Text;
using System.Runtime.CompilerServices;
using Byrone.Xenia.Utilities;

namespace Byrone.Xenia
{
	public static class Cache
	{
		/// <summary>
		/// Returns the needed cache headers based on the given <paramref name="cacheable"/>.
		/// </summary>
		/// <param name="buffer">The byte buffer to fill with the UTF-8 cache headers.</param>
		/// <param name="cacheable">Configured cache settings for the content.</param>
		/// <returns>The generated cache headers.</returns>
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
			builder.AppendFormatted("\nETag: "u8);
			builder.AppendFormatted(cacheable.ETag);

			if (!cacheable.Vary.IsEmpty)
			{
				builder.AppendFormatted("\nVary: "u8);
				builder.AppendFormatted(cacheable.Vary);
			}

			// If the cacheable has a valid 'LastModified' value, the headers should contain this value...
			// ...AND the `Date` header containing the current timestamp.
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

		/// <summary>
		/// Validates if the client's cached content is stale and needs refreshing.
		/// </summary>
		/// <param name="request">The incoming request.</param>
		/// <param name="cacheable">Configured cache settings for the content.</param>
		/// <returns><see langword="true"/> if the cached content needs refreshing, <see langword="false"/> if the content is the same.</returns>
		/// <remarks>
		/// If this function returns <see langword="false"/>, the server is expected to send a response with the
		/// statuscode "304 Not Modified" and the correct cache headers according to the declared <paramref name="cacheable"/>.
		/// </remarks>
		public static bool IsStale(in Request request, Cacheable cacheable)
		{
			// Content isn't allowed to be cached, always return the content.
			if (cacheable.Type == CacheType.NoCache)
			{
				return true;
			}

			if (request.TryGetHeader("If-None-Match"u8, out var ifNoneMatch))
			{
				// If the ETags don't match, the content has changed.
				return !System.MemoryExtensions.SequenceEqual(cacheable.ETag, ifNoneMatch);
			}

			if (cacheable.LastModified != default &&
				request.TryGetHeader("If-Modified-Since"u8, out var ifModifiedSinceHeader) &&
				Cache.TryParseDateTime(ifModifiedSinceHeader, out var ifModifiedSince))
			{
				// If the `LastModified` mark is higher than the given `If-Modified-Since`, the content has changed.
				return cacheable.LastModified > ifModifiedSince;
			}

			// No known cache headers? We can't be sure if the browser has cached data, send the content.
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool TryParseDateTime(scoped System.ReadOnlySpan<byte> buffer, out System.DateTime result) =>
			Utf8Parser.TryParse(buffer, out result, out _, 'R');
	}
}
