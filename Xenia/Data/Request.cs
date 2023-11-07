using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Byrone.Xenia.Helpers;
using JetBrains.Annotations;

namespace Byrone.Xenia.Data
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
#if DEBUG
	[DebuggerTypeProxy(typeof(Request.DebugView))]
#endif
	public readonly struct Request
	{
		public required HttpMethod Method { get; init; }

		public required BytePointer Path { get; init; }

		public required RentedArray<KeyValue> RouteParameters { get; init; }

		public required BytePointer Query { get; init; }

		public required BytePointer HttpVersion { get; init; }

		public required RentedArray<KeyValue> Headers { get; init; }

		public required BytePointer Body { get; init; }

		public required CompressionMethod SupportedCompression { get; init; }

		// @todo Remove
		/// <summary>
		/// DO NOT CALL THIS MANUALLY!
		/// </summary>
		public required RequestHandler.RequestHandlerCallback HandlerCallback { get; init; }

		public void Dispose()
		{
			this.RouteParameters.Dispose();
			this.Headers.Dispose();
		}

#if DEBUG
		private sealed class DebugView
		{
			public readonly HttpMethod Method;
			public readonly string? Path;
			public readonly string? Query;
			public readonly string? HttpVersion;
			public readonly Dictionary<string, string?> RouteParameters;
			public readonly Dictionary<string, string?> Headers;
			public readonly string? Body;
			public readonly CompressionMethod SupportedCompression;

			public DebugView(Request request)
			{
				this.Method = request.Method;
				this.Path = request.Path.ToString();
				this.Query = request.Query.ToString();
				this.HttpVersion = request.HttpVersion.ToString();

				this.Headers = new Dictionary<string, string?>(request.Headers.Size, System.StringComparer.Ordinal);

				foreach (var header in request.Headers)
				{
					var key = header.Key.ToString();

					if (key is null)
					{
						continue;
					}

					var value = header.Value.ToString();

					this.Headers.Add(key, value);
				}

				this.RouteParameters =
					new Dictionary<string, string?>(request.RouteParameters.Size, System.StringComparer.Ordinal);

				foreach (var param in request.RouteParameters)
				{
					var key = param.Key.ToString();

					if (key is null)
					{
						continue;
					}

					var value = param.Value.ToString();

					this.RouteParameters.Add(key, value);
				}

				this.Body = request.Body.ToString();
				this.SupportedCompression = request.SupportedCompression;
			}
		}
#endif
	}
}
