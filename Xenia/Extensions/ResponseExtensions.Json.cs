using System.Runtime.CompilerServices;
using Byrone.Xenia.Data;
using Byrone.Xenia.Helpers;

namespace Byrone.Xenia.Extensions
{
	public static partial class ResponseExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendJson(this ref ResponseBuilder @this,
									  in Request request,
									  in StatusCode code,
									  System.ReadOnlySpan<byte> bytes)
		{
			@this.AppendHeaders(in code,
								request.HttpVersion,
								request.GetCompressionMethodHeader(),
								ContentTypes.Json,
								bytes.Length);

			@this.Append(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AppendJson<T>(this ref ResponseBuilder @this,
										 in Request request,
										 in StatusCode code,
										 T data) where T : IJson<T>
		{
			var bytes = Json.Serialize(data);

			ResponseExtensions.AppendJson(ref @this, in request, in code, bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Append<T>(this ref ResponseBuilder @this, T item) where T : IJson<T> =>
			@this.Append(Json.Serialize(item));
	}
}
