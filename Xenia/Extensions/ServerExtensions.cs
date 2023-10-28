using Byrone.Xenia.Data;
using Byrone.Xenia.Helpers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;

namespace Byrone.Xenia.Extensions
{
	[PublicAPI]
	public static class ServerExtensions
	{
		public static void AddRazorPage<T>(this Server server, System.ReadOnlySpan<byte> path) where T : IComponent =>
			server.AddRequestHandler(new RequestHandler(path, ServerExtensions.RazorRequestHandler<T>));

		private static void RazorRequestHandler<T>(in Request request, ref ResponseBuilder builder) where T : IComponent
		{
			// @todo ResizableRentedArray
			var renderBuffer = new RentedArray<byte>(1024);

			using (var writer = new ByteArrayWriter(renderBuffer.Data))
			{
				HtmlRenderer.RenderAsync<T>(request, writer).GetAwaiter().GetResult();

				builder.AppendHtml(in request, in StatusCodes.Status200OK, writer.Data);
			}

			renderBuffer.Dispose();
		}
	}
}
