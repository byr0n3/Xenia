using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Byrone.Xenia.Helpers;
using JetBrains.Annotations;

namespace Byrone.Xenia.Data
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
	[DebuggerTypeProxy(typeof(Request.DebugView))]
	public readonly struct Request
	{
		public required HttpMethod Method { get; init; }

		public required SpanPointer<byte> Path { get; init; }

		public required SpanPointer<byte> HtmlVersion { get; init; }

		public required RentedArray<RequestHeader> HeaderData { get; init; }

		public required int HeaderCount { get; init; }

		public required SpanPointer<byte> Body { get; init; }

		public System.ReadOnlySpan<RequestHeader> Headers =>
			this.HeaderData.AsSpan(0, this.HeaderCount);

		public bool TryGetHeader(System.ReadOnlySpan<byte> key, out RequestHeader header)
		{
			foreach (var head in this.Headers)
			{
				if (!System.MemoryExtensions.SequenceEqual(head.Key, key))
				{
					continue;
				}

				header = head;
				return true;
			}

			header = default;
			return false;
		}

		public void Dispose() =>
			this.HeaderData.Dispose();

		private sealed class DebugView
		{
			private static readonly System.Text.Encoding encoding = System.Text.Encoding.Latin1;

			public readonly HttpMethod Method;
			public readonly string Path;
			public readonly string HtmlVersion;
			public readonly Dictionary<string, string> Headers;
			public readonly string? Body;

			public DebugView(Request request)
			{
				this.Method = request.Method;
				this.Path = DebugView.encoding.GetString(request.Path.AsSpan);
				this.HtmlVersion = DebugView.encoding.GetString(request.HtmlVersion.AsSpan);

				this.Headers = new Dictionary<string, string>(request.HeaderCount, System.StringComparer.Ordinal);

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
			}
		}
	}
}
