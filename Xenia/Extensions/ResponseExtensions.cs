using System.Runtime.CompilerServices;
using Byrone.Xenia.Data;
using Byrone.Xenia.Helpers;
using Byrone.Xenia.Internal;
using JetBrains.Annotations;

namespace Byrone.Xenia.Extensions
{
	[PublicAPI]
	public static partial class ResponseExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendSpace(this ref ResponseBuilder @this) =>
			@this.Append(' ');

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendColon(this ref ResponseBuilder @this) =>
			@this.Append(':');

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendLineEnd(this ref ResponseBuilder @this)
		{
			@this.Append(Characters.Reset);
			@this.Append(Characters.NewLine);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendHttp(ref this ResponseBuilder @this,
									  System.ReadOnlySpan<byte> httpVersion,
									  in StatusCode statusCode)
		{
			@this.Append(httpVersion);
			@this.AppendSpace();
			@this.Append(statusCode.Code);
			@this.AppendSpace();
			@this.Append(statusCode.Message);
			@this.AppendLineEnd();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendHeader(ref this ResponseBuilder @this,
										System.ReadOnlySpan<byte> key,
										System.ReadOnlySpan<byte> value)
		{
			@this.Append(key);
			@this.AppendColon();
			@this.AppendSpace();
			@this.Append(value);
			@this.AppendLineEnd();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendHeaders(this ref ResponseBuilder @this,
										 in Request request,
										 in StatusCode code,
										 System.ReadOnlySpan<byte> contentType,
										 int contentLength = 0) =>
			ResponseExtensions.AppendHeaders(ref @this,
											 in code,
											 request.HttpVersion,
											 request.GetCompressionMethodHeader(),
											 contentType,
											 contentLength);

		public static void AppendHeaders(this ref ResponseBuilder @this,
										 in StatusCode statusCode,
										 System.ReadOnlySpan<byte> httpVersion,
										 System.ReadOnlySpan<byte> contentEncoding,
										 System.ReadOnlySpan<byte> contentType,
										 int contentLength = 0)
		{
			ResponseExtensions.AppendHttp(ref @this, httpVersion, in statusCode);

			// Headers

			@this.Append(Headers.Date);
			@this.AppendColon();
			@this.AppendSpace();
			@this.Append(System.DateTime.UtcNow);
			@this.AppendLineEnd();

			ResponseExtensions.AppendHeader(ref @this, Headers.Server, "Xenia"u8);

			if (!contentType.IsEmpty)
			{
				ResponseExtensions.AppendHeader(ref @this, Headers.ContentType, contentType);
			}

			if (contentLength > 0)
			{
				@this.Append(Headers.ContentLength);
				@this.AppendColon();
				@this.AppendSpace();
				@this.Append(contentLength);
				@this.AppendLineEnd();
			}

			if (!contentEncoding.IsEmpty)
			{
				ResponseExtensions.AppendHeader(ref @this, Headers.ContentEncoding, contentEncoding);
			}

			@this.AppendLineEnd();
			@this.StartContent();
		}
	}
}
