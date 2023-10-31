using System.Runtime.CompilerServices;
using System.Text;
using Byrone.Xenia.Data;
using Byrone.Xenia.Helpers;
using JetBrains.Annotations;

namespace Byrone.Xenia.Extensions
{
	[PublicAPI]
	public static partial class ResponseExtensions
	{
		private static readonly Encoding enc = Encoding.UTF8;

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

		public static void AppendHeaders(this ref ResponseBuilder @this,
										 in Request request,
										 in StatusCode code,
										 System.ReadOnlySpan<byte> contentType,
										 int contentLength = 0) =>
			ResponseExtensions.AppendHeaders(ref @this,
											 in code,
											 request.HtmlVersion,
											 request.GetCompressionMethodHeader(),
											 contentType,
											 contentLength);

		public static void AppendHeaders(this ref ResponseBuilder @this,
										 in StatusCode code,
										 System.ReadOnlySpan<byte> htmlVersion,
										 System.ReadOnlySpan<byte> contentEncoding,
										 System.ReadOnlySpan<byte> contentType,
										 int contentLength = 0)
		{
			// HTML spec

			@this.Append(htmlVersion);
			@this.AppendSpace();
			@this.Append(code.Code);
			@this.AppendSpace();
			@this.Append(code.Message);
			@this.AppendLineEnd();

			// Headers

			@this.Append(Headers.Date);
			@this.AppendColon();
			@this.AppendSpace();
			@this.Append(System.DateTime.UtcNow);
			@this.AppendLineEnd();

			@this.Append(Headers.Server);
			@this.AppendColon();
			@this.AppendSpace();
			@this.Append("Xenia"u8);
			@this.AppendLineEnd();

			if (!contentType.IsEmpty)
			{
				@this.Append(Headers.ContentType);
				@this.AppendColon();
				@this.AppendSpace();
				@this.Append(contentType);
				@this.AppendLineEnd();
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
				@this.Append(Headers.ContentEncoding);
				@this.AppendColon();
				@this.AppendSpace();
				@this.Append(contentEncoding);
				@this.AppendLineEnd();
			}

			@this.AppendLineEnd();
			@this.StartContent();
		}
	}
}
