using System.Runtime.CompilerServices;
using Byrone.Xenia.Data;
using Byrone.Xenia.Helpers;

namespace Byrone.Xenia.Extensions
{
	public static partial class ResponseExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendJson<T>(this ref ResponseBuilder @this,
										 in Request request,
										 in StatusCode code,
										 T data) where T : IJson<T>
		{
			var bytes = Json.Serialize(data);

			// @todo request.TryGetHeader("Accept-Encoding"u8, out var encodingHeader)
			@this.AppendHeaders(in code, System.ReadOnlySpan<byte>.Empty, "application/json"u8, bytes.Length);

			@this.Append(bytes);
		}
	}
}
