using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using Byrone.Utilities.Text;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;
using JetBrains.Annotations;

namespace Byrone.Xenia
{
	[PublicAPI]
	public sealed class ResponseWriter : System.IDisposable
	{
		private static readonly Encoding encoding = Encoding.Latin1;

		private readonly NetworkStream stream;
		private readonly RentedArray<byte> buffer;

		public ResponseWriter(ref NetworkStream stream, int size = 1024)
		{
			this.stream = stream;
			this.buffer = new RentedArray<byte>(size);
		}

		public void WriteNotFound(in Request request)
		{
			var builder = new ByteArrayBuilder(this.buffer.Data);

			ResponseWriter.AppendBaseResponse(ref builder,
											  StatusCodes.Status404NotFound,
											  ResponseWriter.GetContentEncoding(in request),
											  System.ReadOnlySpan<byte>.Empty);

			this.stream.Write(builder.AsReadOnlySpan());
		}

		public void WriteMethodNotAllowed(in Request request)
		{
			var builder = new ByteArrayBuilder(this.buffer.Data);

			ResponseWriter.AppendBaseResponse(ref builder,
											  StatusCodes.Status405MethodNotAllowed,
											  ResponseWriter.GetContentEncoding(in request),
											  System.ReadOnlySpan<byte>.Empty);

			this.stream.Write(builder.AsReadOnlySpan());
		}

		private static void AppendBaseResponse(ref ByteArrayBuilder builder,
											   StatusCode code,
											   System.ReadOnlySpan<byte> contentEncoding,
											   System.ReadOnlySpan<byte> contentType)
		{
			// @todo Use HTTP version from request
			builder.Append("HTTP/1.1 "u8);
			builder.Append(code.Code);
			ResponseWriter.AppendSpace(ref builder);
			builder.Append(code.Message);
			ResponseWriter.AppendLineEnd(ref builder);

			builder.Append("Date: "u8);
			builder.Append(ResponseWriter.encoding.GetBytes(System.DateTime.UtcNow.ToString()));
			ResponseWriter.AppendLineEnd(ref builder);

			// @todo Configurable
			builder.Append("Server: Xenia"u8);
			ResponseWriter.AppendLineEnd(ref builder);

			if (!contentEncoding.IsEmpty)
			{
				builder.Append("Content-Encoding: "u8);
				builder.Append(contentEncoding);
				ResponseWriter.AppendLineEnd(ref builder);
			}

			if (!contentType.IsEmpty)
			{
				builder.Append("Content-Type: "u8);
				builder.Append(contentType);
				ResponseWriter.AppendLineEnd(ref builder);
			}

			ResponseWriter.AppendLineEnd(ref builder);
		}

		private static System.ReadOnlySpan<byte> GetContentEncoding(in Request request)
		{
			if (request.HeaderData.TryGetHeader("Accept-Encoding"u8, out var header))
			{
				return header.Value;
			}

			return System.ReadOnlySpan<byte>.Empty;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void AppendSpace(ref ByteArrayBuilder builder) =>
			builder.Append((byte)' ');

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void AppendLineEnd(ref ByteArrayBuilder builder) =>
			builder.Append("\r\n"u8);

		public void Dispose() =>
			this.buffer.Dispose();
	}
}
