using System.Runtime.CompilerServices;
using Byrone.Xenia.Data;
using Byrone.Xenia.Helpers;

namespace Byrone.Xenia.Extensions
{
	public static partial class ResponseExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendHtmlHeaders(this ref ResponseBuilder @this, in Request request, in StatusCode code) =>
			@this.AppendHeaders(in request, in code, ContentTypes.Html);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendHtml(this ref ResponseBuilder @this,
									  in Request request,
									  in StatusCode code,
									  System.ReadOnlySpan<char> html)
		{
			@this.AppendHtmlHeaders(in request, in code);

			@this.Append(html);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendHtml(this ref ResponseBuilder @this,
									  in Request request,
									  in StatusCode code,
									  System.ReadOnlySpan<byte> html)
		{
			@this.AppendHtmlHeaders(in request, in code);

			@this.Append(html);
		}
	}
}
