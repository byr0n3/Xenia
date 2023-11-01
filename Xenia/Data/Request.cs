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

		public required SpanPointer<byte> Path { get; init; }

		public required SpanPointer<byte> Query { get; init; }

		public required SpanPointer<byte> HtmlVersion { get; init; }

		public required RentedArray<KeyValue> Headers { get; init; }

		public required SpanPointer<byte> Body { get; init; }

		public required CompressionMethod SupportedCompression { get; init; }

		public void Dispose() =>
			this.Headers.Dispose();

#if DEBUG
		private sealed class DebugView
		{
			private static readonly System.Text.Encoding encoding = System.Text.Encoding.Latin1;

			public readonly HttpMethod Method;
			public readonly string Path;
			public readonly string HtmlVersion;
			public readonly Dictionary<string, string> Headers;
			public readonly string? Body;
			public readonly CompressionMethod SupportedCompression;

			public DebugView(Request request)
			{
				this.Method = request.Method;
				this.Path = DebugView.encoding.GetString(request.Path.Span);
				this.HtmlVersion = DebugView.encoding.GetString(request.HtmlVersion.Span);

				this.Headers = new Dictionary<string, string>(request.Headers.Size, System.StringComparer.Ordinal);

				foreach (var header in request.Headers)
				{
					var key = DebugView.encoding.GetString(header.Key);
					var value = DebugView.encoding.GetString(header.Value);

					this.Headers.Add(key, value);
				}

				if (request.Body != default)
				{
					this.Body = DebugView.encoding.GetString(request.Body);
				}

				this.SupportedCompression = request.SupportedCompression;
			}
		}
#endif
	}
}
