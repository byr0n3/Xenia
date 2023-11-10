using System.Runtime.CompilerServices;
using Byrone.Xenia.Data;
using Byrone.Xenia.Helpers;
using Byrone.Xenia.Internal;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;

namespace Byrone.Xenia.Extensions
{
	[PublicAPI]
	public static class ServerExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddRazorPage<T>(this Server server, System.ReadOnlySpan<byte> path) where T : IComponent =>
			server.AddRequestHandler(new RequestHandler(path, ServerExtensions.RazorRequestHandler<T>));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool RemoveRazorPage<T>(this Server server, System.ReadOnlySpan<byte> path) where T : IComponent =>
			server.RemoveRequestHandler(new RequestHandler(path, ServerExtensions.RazorRequestHandler<T>));

		// @todo Strip new lines/empty lines?
		private static void RazorRequestHandler<T>(in Request request, ref ResponseBuilder builder) where T : IComponent
		{
			var writer = new ByteArrayWriter();

			HtmlRenderer.RenderAsync<T>(request, writer).GetAwaiter().GetResult();

			builder.AppendHtml(in request, in StatusCodes.Status200OK, writer.Data);

			writer.Dispose();
		}
	}
}
