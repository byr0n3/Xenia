using System.IO;
using System.Threading.Tasks;
using Byrone.Xenia.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Renderer = Microsoft.AspNetCore.Components.Web.HtmlRenderer;

namespace Byrone.Xenia.Helpers
{
	internal static class HtmlRenderer
	{
		public static async Task RenderAsync<T>(Request request, TextWriter writer) where T : IComponent
		{
			var provider = new ServiceCollection().AddCascadingValue((_) => request).BuildServiceProvider();

			var renderer = new Renderer(provider, NullLoggerFactory.Instance);

			await using (renderer.ConfigureAwait(false))
			{
				await renderer.Dispatcher.InvokeAsync(async () =>
				{
					var @out = await renderer.RenderComponentAsync<T>().ConfigureAwait(false);

					@out.WriteHtmlTo(writer);
				}).ConfigureAwait(false);
			}
		}
	}
}
