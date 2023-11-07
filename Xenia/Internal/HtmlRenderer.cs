using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Byrone.Xenia.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Renderer = Microsoft.AspNetCore.Components.Web.HtmlRenderer;

namespace Byrone.Xenia.Internal
{
	internal static class HtmlRenderer
	{
		private static readonly Dictionary<System.Type, CachedLayoutFragment> layoutRenderCache = new();

		public static async Task RenderAsync<T>(Request request, TextWriter writer) where T : IComponent
		{
			var cached = HtmlRenderer.GetCachedLayoutFragment<T>();

			var provider = new ServiceCollection().AddCascadingValue((_) => request).BuildServiceProvider();

			await using (provider.ConfigureAwait(false))
			{
				var renderer = new Renderer(provider, NullLoggerFactory.Instance);

				await using (renderer.ConfigureAwait(false))
				{
					await renderer.Dispatcher.InvokeAsync(async () =>
					{
						HtmlRootComponent @out;

						if (cached.Valid)
						{
							@out = await renderer.RenderComponentAsync(cached.LayoutType, cached.Parameters)
												 .ConfigureAwait(false);
						}
						else
						{
							@out = await renderer.RenderComponentAsync<T>().ConfigureAwait(false);
						}

						@out.WriteHtmlTo(writer);
					}).ConfigureAwait(false);
				}
			}
		}

		private static CachedLayoutFragment GetCachedLayoutFragment<T>() where T : IComponent
		{
			var componentType = typeof(T);

			if (HtmlRenderer.layoutRenderCache.TryGetValue(componentType, out var cached))
			{
				return cached;
			}

			var layout = (LayoutAttribute?)System.Attribute.GetCustomAttribute(componentType, typeof(LayoutAttribute));

			if (layout is null)
			{
				return default;
			}

			var @params = ParameterView.FromDictionary(GetParameterDictionary());

			cached = new CachedLayoutFragment(layout.LayoutType, @params);

			HtmlRenderer.layoutRenderCache.Add(componentType, cached);

			return cached;

			static Dictionary<string, object?> GetParameterDictionary()
			{
				return new Dictionary<string, object?>(System.StringComparer.Ordinal)
				{
					{
						nameof(LayoutComponentBase.Body), (RenderFragment)(static builder =>
						{
							builder.OpenComponent<T>(0);
							builder.CloseComponent();
						})
					},
				};
			}
		}

		private readonly struct CachedLayoutFragment
		{
			public required System.Type LayoutType { get; init; }

			public required ParameterView Parameters { get; init; }

			public required bool Valid { get; init; }

			[SetsRequiredMembers]
			public CachedLayoutFragment(System.Type layoutType, ParameterView parameters)
			{
				this.LayoutType = layoutType;
				this.Parameters = parameters;
				this.Valid = true;
			}
		}
	}
}
