using System.Buffers;
using System.Runtime.InteropServices;
using Byrone.Xenia.Data;
using Byrone.Xenia.Extensions;
using Bytes = System.ReadOnlySpan<byte>;
using Ranges = System.ReadOnlySpan<System.Range>;

namespace Byrone.Xenia.Helpers
{
	internal static class ServerHelpers
	{
		public static bool TryGetRequest(ServerOptions options, Bytes bytes, Ranges ranges, out Request request)
		{
			if (!ServerHelpers.TryGetHtmlCommand(bytes, ranges, out var command))
			{
				request = default;
				return false;
			}

			// @todo Find the range that is just the \r\n separator, index is the amount of headers

			// skip the html command
			var headers = new RentedArray<KeyValue>(ranges.Length - 1);
			var headerCount = ServerHelpers.ParseHeaders(bytes, ranges, ref headers);

			// important: DON'T DISPOSE THE HEADERS VARIABLE!
			// we pass the rented array to a new instance down below

			request = new Request
			{
				Method = command.Method,
				Path = command.Path,
				Query = command.Query,
				HtmlVersion = command.Html,
				// bit of a hack, used to redefine the 'size' property of the RentedArray instance
				// otherwise we'd have to also define something like 'HeadersCount' in the request
				Headers = new RentedArray<KeyValue>(headers.Data, ArrayPool<KeyValue>.Shared, headerCount),
				Body = ServerHelpers.GetRequestBody(command.Method, bytes, ranges),
				SupportedCompression = options.SupportedCompression,
			};

			return true;
		}

		// the first line of the request contains the HTML command, example:
		// GET / HTTP/1.1
		private static bool TryGetHtmlCommand(Bytes bytes, Ranges ranges, out HtmlCommand command)
		{
			const byte space = (byte)' ';

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
			var query = Bytes.Empty;

			var queryIdx = System.MemoryExtensions.IndexOf(path, Characters.QuestionMark);

			if (queryIdx != -1)
			{
				query = path.Slice(queryIdx + 1);
				path = path.Slice(0, queryIdx);
			}

			var htmlIdx = methodIdx + pathIdx + 2;

			var html = line.SliceTrimmed(htmlIdx, line.Length - htmlIdx);

			command = new HtmlCommand
			{
				Method = method,
				Path = path,
				Query = query,
				Html = html,
			};

			return true;
		}

		private static Bytes GetRequestBody(HttpMethod method, Bytes bytes, Ranges ranges)
		{
			if (method is not (HttpMethod.Post or HttpMethod.Put or HttpMethod.Patch))
			{
				return Bytes.Empty;
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

			return newLineIdx == 0 ? Bytes.Empty : bytes.Slice(ranges[newLineIdx].Start.Value);
		}

		private static int ParseHeaders(Bytes bytes, Ranges ranges, ref RentedArray<KeyValue> headers)
		{
			const byte semiColon = (byte)':';
			const int valueOffset = sizeof(byte) * 2;

			var count = 0;

			// start at 1 to skip the HTML command
			for (var i = 1; i < ranges.Length - 1; i++)
			{
				var range = ranges[i];

				// just an empty line/new line character, we've reached the end of the headers
				if (range.End.Value <= 1)
				{
					break;
				}

				var slice = bytes.SliceTrimmed(range);

				var separatorIdx = System.MemoryExtensions.IndexOf(slice, semiColon);

				// invalid header, shouldn't happen
				if (separatorIdx <= 0)
				{
					continue;
				}

				var key = slice.SliceTrimmed(0, separatorIdx);

				// add an offset to skip ': '
				var valueIdx = separatorIdx + valueOffset;
				var value = slice.SliceTrimmed(valueIdx, slice.Length - (valueIdx));

				headers[count++] = new KeyValue(key, value);
			}

			return count;
		}

		private static HttpMethod GetMethod(Bytes bytes)
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

		// header can contain multiple accepted encodings, find the first supported one
		public static bool TryGetValidCompressionMode(Bytes value,
													  CompressionMethod supported,
													  out CompressionMethod @out)
		{
			if (value.IsEmpty)
			{
				@out = default;
				return false;
			}

			var ranges = new RentedArray<System.Range>(3);

			var count = value.Split(ranges.Data, Characters.Comma);

			if (count == 0)
			{
				ranges.Dispose();

				@out = ServerHelpers.GetCompressionMode(value);
				return @out != CompressionMethod.None;
			}

			for (var i = 0; i < count; i++)
			{
				var range = ranges[i];
				var start = range.Start.Value;
				var length = range.End.Value;

				// trim space at the start
				if (i > 0)
				{
					start++;
					length--;
				}

				@out = ServerHelpers.GetCompressionMode(value.Slice(start, length));

				if (@out == CompressionMethod.None || (supported & @out) == CompressionMethod.None)
				{
					continue;
				}

				ranges.Dispose();
				return true;
			}

			ranges.Dispose();

			@out = CompressionMethod.None;
			return false;
		}

		private static CompressionMethod GetCompressionMode(Bytes value)
		{
			// handle `br;q=1.0, gzip;q=0.8, *;q=0.1`, we don't care about the quality value behind the semi colon
			// it's probably in the correct order anyway
			var idx = System.MemoryExtensions.IndexOf(value, Characters.SemiColon);

			if (idx != -1)
			{
				value = value.Slice(0, idx);
			}

			if (System.MemoryExtensions.SequenceEqual(value, "gzip"u8))
			{
				return CompressionMethod.GZip;
			}

			if (System.MemoryExtensions.SequenceEqual(value, "deflate"u8))
			{
				return CompressionMethod.Deflate;
			}

			if (System.MemoryExtensions.SequenceEqual(value, "br"u8) ||
				System.MemoryExtensions.SequenceEqual(value, "brotli"u8))
			{
				return CompressionMethod.Brotli;
			}

			return CompressionMethod.None;
		}

		[StructLayout(LayoutKind.Sequential)]
		private readonly struct HtmlCommand
		{
			public required HttpMethod Method { get; init; }

			public required BytePointer Path { get; init; }

			public required BytePointer Query { get; init; }

			public required BytePointer Html { get; init; }
		}
	}
}
