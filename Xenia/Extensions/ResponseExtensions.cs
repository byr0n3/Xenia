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
		public static void AppendLineEnd(this ref ResponseBuilder @this) =>
			@this.Append("\r\n"u8);

		public static void AppendHeaders(this ref ResponseBuilder @this,
										 in StatusCode code,
										 System.ReadOnlySpan<byte> htmlVersion,
										 System.ReadOnlySpan<byte> contentEncoding,
										 System.ReadOnlySpan<byte> contentType,
										 int contentLength)
		{
			// HTML spec

			@this.Append(htmlVersion);
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

			if (!contentEncoding.IsEmpty)
			{
				@this.Append("Content-Encoding: "u8);
				@this.Append(contentEncoding);
				@this.AppendLineEnd();
			}

			@this.AppendLineEnd();
		}
	}
}
