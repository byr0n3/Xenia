using System.Runtime.CompilerServices;
using Byrone.Xenia.Data;
using Byrone.Xenia.Helpers;

namespace Byrone.Xenia.Extensions
{
	public static partial class ResponseExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendHtmlHeaders(this ref ResponseBuilder @this,
											 in Request request,
											 in StatusCode code,
											 int contentLength) =>
			@this.AppendHeaders(in code,
								request.HtmlVersion,
								System.ReadOnlySpan<byte>.Empty,
								ContentTypes.Html,
								contentLength);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendHtml(this ref ResponseBuilder @this,
									  in Request request,
									  in StatusCode code,
									  System.ReadOnlySpan<char> html)
		{
			@this.AppendHtmlHeaders(in request, in code, ResponseExtensions.enc.GetByteCount(html));

			@this.Append(html);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
