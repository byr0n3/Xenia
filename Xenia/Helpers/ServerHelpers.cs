using System.Runtime.InteropServices;
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
			if (!ServerHelpers.TryGetHtmlCommand(bytes, ranges, out var command))
			{
				request = default;
				return false;
			}

			// skip the html command
			var headers = new RentedArray<RequestHeader>(ranges.Length - 1);
			var headerCount = ServerHelpers.ParseHeaders(bytes, ref headers, ranges);

			request = new Request
			{
				Method = command.Method,
				Path = command.Path,
				HtmlVersion = command.Html,
				HeaderData = headers,
				HeaderCount = headerCount,
				Body = ServerHelpers.GetRequestBody(command.Method, bytes, ranges),
			};

			return true;
		}

		private static bool TryGetHtmlCommand(System.ReadOnlySpan<byte> bytes,
											  System.ReadOnlySpan<System.Range> ranges,
											  out HtmlCommand command)
		{
			const byte space = (byte)' ';

			// the first line of the request contains the HTML command, example:
			// GET / HTTP/1.1

			var line = bytes.Slice(ranges[0]);
			var methodIdx = System.MemoryExtensions.IndexOf(line, space);

			if (methodIdx <= 0)
			{
				command = default;
				return false;
			}

			var method = ServerHelpers.GetMethod(line.Slice(0, methodIdx));

			var pathIdx = System.MemoryExtensions.IndexOf(line.Slice(methodIdx + 1), space);

			if (pathIdx <= 0)
			{
				command = default;
				return false;
			}

			var path = line.Slice(methodIdx + 1, pathIdx);

			var htmlIdx = methodIdx + pathIdx + 2;

			var html = line.Trim(htmlIdx, line.Length - htmlIdx);

			command = new HtmlCommand
			{
				Method = method,
				Path = path,
				Html = html,
			};

			return true;
		}

		private static System.ReadOnlySpan<byte> GetRequestBody(HttpMethod method,
																System.ReadOnlySpan<byte> bytes,
																System.ReadOnlySpan<System.Range> ranges)
		{
			if (method is not (HttpMethod.Post or HttpMethod.Put or HttpMethod.Patch))
			{
				return System.ReadOnlySpan<byte>.Empty;
			}

			// get the index of the line that's just a new line
			// the line after this is the start of the request body
			var newLineIdx = 0;

			for (var i = 1; i < ranges.Length; i++)
			{
				var range = ranges[i];

				if (range.End.Value > 1)
				{
					continue;
				}

				newLineIdx = i + 1;
				break;
			}

			return newLineIdx == 0 ? System.ReadOnlySpan<byte>.Empty : bytes.Slice(ranges[newLineIdx].Start.Value);
		}

		private static int ParseHeaders(System.ReadOnlySpan<byte> bytes,
										ref RentedArray<RequestHeader> headers,
										System.ReadOnlySpan<System.Range> ranges)
		{
			const byte semiColon = (byte)':';
			const int valueOffset = sizeof(byte) * 2;

			var count = 0;

			for (var i = 1; i < ranges.Length - 1; i++)
			{
				var range = ranges[i];

				// just an empty line/new line character, we've reached the end of the headers
				if (range.End.Value <= 1)
				{
					break;
				}

				var slice = bytes.Trim(range);

				var separatorIdx = System.MemoryExtensions.IndexOf(slice, semiColon);

				// invalid header, shouldn't happen
				if (separatorIdx <= 0)
				{
					continue;
				}

				var key = slice.Trim(0, separatorIdx);

				// add an offset to skip ': '
				var valueIdx = separatorIdx + valueOffset;
				var value = slice.Trim(valueIdx, slice.Length - (valueIdx));

				headers[count++] = new RequestHeader(key, value);
			}

			return count;
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

		[StructLayout(LayoutKind.Sequential)]
		private readonly struct HtmlCommand
		{
			public required HttpMethod Method { get; init; }

			public required SpanPointer<byte> Path { get; init; }

			public required SpanPointer<byte> Html { get; init; }
		}
	}
}
