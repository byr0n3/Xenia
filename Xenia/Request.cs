using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Byrone.Xenia.Internal;
using Byrone.Xenia.Utilities;
using JetBrains.Annotations;

namespace Byrone.Xenia
{
	[PublicAPI]
	[MustDisposeResource]
	[StructLayout(LayoutKind.Sequential)]
#if DEBUG
	[System.Diagnostics.DebuggerTypeProxy(typeof(Request.DebugView))]
#endif
	public readonly ref struct Request : System.IDisposable
	{
		public readonly System.ReadOnlySpan<byte> Method;

		public readonly System.ReadOnlySpan<byte> Path;

		public readonly System.ReadOnlySpan<byte> HttpVersion;

		public readonly System.ReadOnlySpan<byte> Body;

		private readonly RentedArray<KeyValuePair<Unmanaged, Unmanaged>> headerData;

		public System.ReadOnlySpan<KeyValuePair<Unmanaged, Unmanaged>> Headers =>
			this.headerData.Span;

		internal Request(System.ReadOnlySpan<byte> method,
						 System.ReadOnlySpan<byte> path,
						 System.ReadOnlySpan<byte> httpVersion,
						 RentedArray<KeyValuePair<Unmanaged, Unmanaged>> headers,
						 System.ReadOnlySpan<byte> body)
		{
			this.Method = method;
			this.Path = path;
			this.HttpVersion = httpVersion;
			this.Body = body;

			this.headerData = headers;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose() =>
			this.headerData.Dispose();

#if DEBUG
		[StructLayout(LayoutKind.Sequential)]
		private sealed class DebugView
		{
			private static readonly System.Text.Encoding enc = System.Text.Encoding.UTF8;

			public readonly string Method;

			public readonly string Path;

			public readonly string HttpVersion;

			public readonly Dictionary<string, string> Headers;

			public readonly string Body;

			public DebugView(Request request)
			{
				this.Method = DebugView.Str(request.Method);
				this.Path = DebugView.Str(request.Path);
				this.HttpVersion = DebugView.Str(request.HttpVersion);
				this.Body = DebugView.Str(request.Body);

				this.Headers = new Dictionary<string, string>(System.StringComparer.Ordinal);

				foreach (var keyValue in request.Headers)
				{
					this.Headers[DebugView.Str(keyValue.Key.Managed)] = DebugView.Str(keyValue.Value.Managed);
				}
			}

			private static string Str(scoped System.ReadOnlySpan<byte> span) =>
				DebugView.enc.GetString(span);
		}
#endif
	}
}
