using System.Buffers;
using System.Runtime.InteropServices;
using Byrone.Xenia.Data;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;
using Bytes = System.ReadOnlySpan<byte>;
using Ranges = System.ReadOnlySpan<System.Range>;

namespace Byrone.Xenia.Internal
{
	internal static class ServerHelpers
	{
		public static bool TryGetRequest(Server server, Bytes bytes, Ranges ranges, out Request request)
		{
			if (!ServerHelpers.TryGetHttpCommand(bytes, ranges, out var command))
			{
				request = default;
				return false;
			}

			var handler = server.GetRequestHandler(command.Method, command.Path, out var routeParameters);

			// @todo Find the range that is just the \r\n separator, index is the amount of headers

			// skip the http command
			var headers = new RentedArray<KeyValue>(ranges.Length - 1);
			var headerCount = ServerHelpers.ParseHeaders(bytes, ranges, ref headers);

			// bit of a hack, used to redefine the 'size' property of the RentedArray instance
			// otherwise we'd have to also define something like 'HeadersCount' in the request
			headers = new RentedArray<KeyValue>(headers.Data, ArrayPool<KeyValue>.Shared, headerCount);

			var body = ServerHelpers.GetRequestBody(command.Method, bytes, ranges);

			// important: DON'T DISPOSE THE HEADERS VARIABLE!
			// we pass the rented array to a new instance down below

			request = new Request
			{
				Method = command.Method,
				Path = command.Path,
				RouteParameters = routeParameters,
				Query = command.Query,
				HttpVersion = command.Http,
				Headers = headers,
				Body = body,
				SupportedCompression = server.Options.SupportedCompression,
				HandlerCallback = handler,
			};

			return true;
		}

		// the first line of the request contains the HTTP command, example:
		// GET / HTTP/1.1
		private static bool TryGetHttpCommand(Bytes bytes, Ranges ranges, out HttpCommand command)
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

			var httpIdx = methodIdx + pathIdx + 2;

			var http = line.SliceTrimmed(httpIdx, line.Length - httpIdx);

			command = new HttpCommand
			{
				Method = method,
				Path = path,
				Query = query,
				Http = http,
			};

			return true;
		}

		private static Bytes GetRequestBody(HttpMethod method, Bytes bytes, Ranges ranges)
		{
			if (method is not (HttpMethod.Post or HttpMethod.Put or HttpMethod.Patch))
			{
				return default;
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

			return newLineIdx != 0 ? bytes.Slice(ranges[newLineIdx].Start.Value) : default;
		}

		private static int ParseHeaders(Bytes bytes, Ranges ranges, ref RentedArray<KeyValue> headers)
		{
			const byte semiColon = (byte)':';
			const int valueOffset = sizeof(byte) * 2;

			var count = 0;

			// start at 1 to skip the HTTP command
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

		// @todo Refactor
		public static bool ComparePaths(Bytes requestPath, Bytes handlerPath, out RentedArray<KeyValue> parameters)
		{
			var paramIdx = System.MemoryExtensions.IndexOf(handlerPath, Characters.OpenCurlyBracket);

			if (paramIdx == -1)
			{
				parameters = default;
				return System.MemoryExtensions.SequenceEqual(requestPath, handlerPath);
			}

			System.Span<System.Range> handlerRanges = stackalloc System.Range[16];
			var handlerCount = handlerPath.Split(handlerRanges, Characters.ForwardSlash);

			if (handlerCount == 0)
			{
				parameters = default;
				return System.MemoryExtensions.SequenceEqual(requestPath, handlerPath);
			}

			System.Span<System.Range> requestRanges = stackalloc System.Range[16];
			var requestCount = requestPath.Split(requestRanges, Characters.ForwardSlash);

			if (handlerCount != requestCount)
			{
				parameters = default;
				return false;
			}

			var parameterCount = System.MemoryExtensions.Count(handlerPath, Characters.OpenCurlyBracket);
			parameters = new RentedArray<KeyValue>(parameterCount);
			var currentParam = 0;

			for (var i = 0; i < handlerCount; i++)
			{
				var handlerRange = handlerRanges[i];
				var requestRange = requestRanges[i];

				var handlerPart = handlerPath.Slice(handlerRange);
				var requestPart = requestPath.Slice(requestRange);

				var openParamIdx = System.MemoryExtensions.IndexOf(handlerPart, Characters.OpenCurlyBracket);

				if (openParamIdx == -1)
				{
					if (System.MemoryExtensions.SequenceEqual(handlerPart, requestPart))
					{
						continue;
					}

					parameters = default;
					return false;
				}

				// skip { and }
				var paramName = handlerPart.Slice(1, handlerPart.Length - 2);

				parameters[currentParam++] = new KeyValue(paramName, requestPart);
			}

			return true;
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

			System.Span<System.Range> ranges = stackalloc System.Range[3];

			var count = value.Split(ranges, Characters.Comma);

			if (count == 0)
			{
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

				return true;
			}

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
		private readonly struct HttpCommand
		{
			public required HttpMethod Method { get; init; }

			public required BytePointer Path { get; init; }

			public required BytePointer Query { get; init; }

			public required BytePointer Http { get; init; }
		}
	}
}
