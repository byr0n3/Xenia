using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Byrone.Xenia
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
	[DebuggerTypeProxy(typeof(Request.DebugView))]
	public readonly ref struct Request
	{
		public required HttpMethod Method { get; init; }

		public required System.ReadOnlySpan<byte> Path { get; init; }

		public required RentedArray<RequestHeader> HeaderData { get; init; }

		public required int HeaderCount { get; init; }

		public System.ReadOnlySpan<RequestHeader> Headers =>
			this.HeaderData.AsSpan(0, this.HeaderCount);

		public void Dispose()
		{
			this.HeaderData.Dispose();
		}

		private sealed class DebugView
		{
			private static readonly System.Text.Encoding encoding = System.Text.Encoding.Latin1;

			public readonly HttpMethod Method;
			public readonly string Path;
			public readonly Dictionary<string, string> Headers;

			public DebugView(Request request)
			{
				this.Method = request.Method;
				this.Path = DebugView.encoding.GetString(request.Path);

				this.Headers = new Dictionary<string, string>(request.HeaderCount, System.StringComparer.Ordinal);

				foreach (var header in request.Headers)
				{
					this.Headers.Add(DebugView.encoding.GetString(header.Key),
									 DebugView.encoding.GetString(header.Value));
				}
			}
		}
	}
}
