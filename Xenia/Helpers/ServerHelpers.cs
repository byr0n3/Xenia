using Byrone.Xenia.Data;
using Byrone.Xenia.Extensions;

namespace Byrone.Xenia.Helpers
{
	internal static class ServerHelpers
	{
		public static bool TryGetRequest(System.ReadOnlySpan<byte> bytes,
										 System.ReadOnlySpan<System.Range> ranges,
										 out Request request)
		{
			const byte space = (byte)' ';
			const byte semiColon = (byte)':';

			var first = bytes.Slice(ranges[0]);
			var methodIdx = System.MemoryExtensions.IndexOf(first, space);

			if (methodIdx <= 0)
			{
				request = default;
				return false;
			}

			var method = ServerHelpers.GetMethod(first.Slice(0, methodIdx));

			var pathIdx = System.MemoryExtensions.IndexOf(first.Slice(methodIdx + 1), space);

			if (pathIdx <= 0)
			{
				request = default;
				return false;
			}

			var path = new System.Range(methodIdx + 1, pathIdx);

			// skip the first row
			var headers = new RentedArray<RequestHeader>(ranges.Length - 1);
			var headerCount = 0;

			for (var i = 1; i < ranges.Length - 1; i++)
			{
				var range = ranges[i];

				// without the \r character
				var slice = bytes.Slice(range.Start.Value, range.End.Value - 1);
				var separatorIdx = System.MemoryExtensions.IndexOf(slice, semiColon);

				if (separatorIdx <= 0)
				{
					continue;
				}

				var key = slice.Slice(0, separatorIdx);
				var value = slice.Slice(separatorIdx + 2, slice.Length - (separatorIdx + 2));

				headers[headerCount++] = new RequestHeader(key, value);
			}

			request = new Request
			{
				Method = method,
				Path = first.Slice(path),
				HeaderData = headers,
				HeaderCount = headerCount,
				Body = method is HttpMethod.Post or HttpMethod.Put or HttpMethod.Patch
					? bytes.Slice(ranges[^1])
					: default,
			};

			return true;
		}

		// @todo Refactor
		private static HttpMethod GetMethod(System.ReadOnlySpan<byte> bytes)
		{
			if (System.MemoryExtensions.SequenceEqual(bytes, "GET"u8))
			{
				return HttpMethod.Get;
			}

			if (System.MemoryExtensions.SequenceEqual(bytes, "POST"u8))
			{
				return HttpMethod.Post;
			}

			if (System.MemoryExtensions.SequenceEqual(bytes, "HEAD"u8))
			{
				return HttpMethod.Head;
			}

			if (System.MemoryExtensions.SequenceEqual(bytes, "PUT"u8))
			{
				return HttpMethod.Put;
			}

			if (System.MemoryExtensions.SequenceEqual(bytes, "DELETE"u8))
			{
				return HttpMethod.Delete;
			}

			if (System.MemoryExtensions.SequenceEqual(bytes, "PATCH"u8))
			{
				return HttpMethod.Patch;
			}

			if (System.MemoryExtensions.SequenceEqual(bytes, "OPTIONS"u8))
			{
				return HttpMethod.Options;
			}

			return HttpMethod.None;
		}
	}
}
