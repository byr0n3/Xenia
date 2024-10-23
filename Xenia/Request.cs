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

		/// <summary>
		/// Get the value of the header defined by <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The name of the header to find.</param>
		/// <returns>The header value when found, <see langword="default"/> otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public System.ReadOnlySpan<byte> GetHeader(scoped System.ReadOnlySpan<byte> key) =>
			this.TryGetHeader(key, out var result) ? result : default;

		/// <summary>
		/// Try to get the value of the header defined by <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The name of the header to find.</param>
		/// <param name="result">The header value when found, <see langword="default"/> otherwise.</param>
		/// <returns><see langword="true"/> if the header was found, <see langword="false"/> otherwise.</returns>
		public bool TryGetHeader(scoped System.ReadOnlySpan<byte> key, out System.ReadOnlySpan<byte> result)
		{
			foreach (var header in this.Headers)
			{
				if (System.MemoryExtensions.SequenceEqual(header.Key, key))
				{
					result = header.Value;
					return true;
				}
			}

			result = default;
			return false;
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
