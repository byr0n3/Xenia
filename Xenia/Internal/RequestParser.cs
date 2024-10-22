using System.Collections.Generic;
using Byrone.Xenia.Utilities;

namespace Byrone.Xenia.Internal
{
	internal static class RequestParser
	{
		private static System.ReadOnlySpan<byte> BodySeparator =>
			"\n\n"u8;

		public static bool TryParse(System.ReadOnlySpan<byte> data, out Request request)
		{
			var bodyIdx = System.MemoryExtensions.IndexOf(data, RequestParser.BodySeparator);
			var body = bodyIdx == -1 ? default : data.Slice(bodyIdx + RequestParser.BodySeparator.Length);

			if (bodyIdx != -1)
			{
				data = data.Slice(0, bodyIdx);
			}

			var enumerator = new SplitEnumerator(data, (byte)'\n');
			enumerator.MoveNext();

			var httpEnumerator = new SplitEnumerator(enumerator.Current, (byte)' ');
			var method = httpEnumerator.MoveNextAndGet();
			var path = httpEnumerator.MoveNextAndGet();
			var httpVersion = httpEnumerator.MoveNextAndGet();

			var headers = RequestParser.GetHeaders(enumerator);

			request = new Request(method, path, httpVersion, headers, body);
			return true;
		}

		private static RentedArray<KeyValuePair<Unmanaged, Unmanaged>> GetHeaders(SplitEnumerator enumerator)
		{
			var headers = new RentedArray<KeyValuePair<Unmanaged, Unmanaged>>(enumerator.Count());

			var count = 0;

			foreach (var header in enumerator)
			{
				var separator = System.MemoryExtensions.IndexOf(header, (byte)':');

				if (separator == -1)
				{
					// End of HTTP headers
					break;
				}

				var key = header.Slice(0, separator);
				var value = header.Slice(separator + 1);

				if (value[0] == (byte)' ')
				{
					value = value.Slice(1);
				}

				headers[count++] = new KeyValuePair<Unmanaged, Unmanaged>(key, value);
			}

			return headers;
		}
	}
}
