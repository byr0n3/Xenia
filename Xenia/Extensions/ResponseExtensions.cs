using System.Runtime.CompilerServices;
using System.Text;
using Byrone.Xenia.Data;
using Byrone.Xenia.Helpers;
using JetBrains.Annotations;

namespace Byrone.Xenia.Extensions
{
	[PublicAPI]
	public static class ResponseExtensions
	{
		private static readonly Encoding enc = Encoding.UTF8;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendSpace(this ref ResponseBuilder @this) =>
			@this.Append(' ');

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendLineEnd(this ref ResponseBuilder @this) =>
			@this.Append("\r\n"u8);

		public static void AppendHeaders(this ref ResponseBuilder @this,
										 in StatusCode code,
										 System.ReadOnlySpan<byte> contentEncoding,
										 System.ReadOnlySpan<byte> contentType,
										 int contentLength)
		{
			// HTML spec

			// @todo Get from request
			@this.Append("HTTP/1.1"u8);
			@this.AppendSpace();
			@this.Append(code.Code);
			@this.AppendSpace();
			@this.Append(code.Message);
			@this.AppendLineEnd();

			// Headers

			@this.Append("Date: "u8);
			@this.Append(System.DateTime.UtcNow);
			@this.AppendLineEnd();

			@this.Append("Server: Xenia"u8);
			@this.AppendLineEnd();

			@this.Append("Content-Type: "u8);
			@this.Append(contentType);
			@this.AppendLineEnd();

			@this.Append("Content-Length: "u8);
			@this.Append(contentLength);
			@this.AppendLineEnd();

			// @todo Encoding/compression support (gzip breaks on Safari, MacOS)
			if (!contentEncoding.IsEmpty)
			{
				@this.Append("Content-Encoding: "u8);
				@this.Append(contentEncoding);
				@this.AppendLineEnd();
			}

			@this.AppendLineEnd();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendHtmlHeaders(this ref ResponseBuilder @this,
											 in Request request,
											 in StatusCode code,
											 int contentLength) =>
			// @todo request.TryGetHeader("Accept-Encoding"u8, out var encodingHeader)
			@this.AppendHeaders(in code, System.ReadOnlySpan<byte>.Empty, "text/html"u8, contentLength);

		public static void AppendHtml(this ref ResponseBuilder @this,
									  in Request request,
									  in StatusCode code,
									  System.ReadOnlySpan<char> html)
		{
			@this.AppendHtmlHeaders(in request, in code, ResponseExtensions.enc.GetByteCount(html));

			@this.Append(html);
		}

		public static void AppendHtml(this ref ResponseBuilder @this,
									  in Request request,
									  in StatusCode code,
									  System.ReadOnlySpan<byte> html)
		{
			@this.AppendHtmlHeaders(in request, in code, html.Length);

			@this.Append(html);
		}
	}
}
