using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Byrone.Xenia.Internal;
using Byrone.Xenia.Internal.Extensions;
using JetBrains.Annotations;

namespace Byrone.Xenia.Utilities
{
	/// <summary>
	/// Helper struct for retrieving and parsing route parameters using a specified pattern and the requested path.
	/// </summary>
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
	public readonly ref struct RouteParameters
	{
		private readonly System.ReadOnlySpan<byte> pattern;
		private readonly System.ReadOnlySpan<byte> path;

		/// <summary>
		/// Create a new <see cref="RouteParameters"/> instance.
		/// </summary>
		/// <param name="pattern">The pattern/route to compare the <paramref name="path"/> to.</param>
		/// <param name="path">The requested path.</param>
		/// <example><c>var routeParams = new RouteParameters("/blog/{post}/view"u8, "/blog/hello-world/view"u8);</c></example>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RouteParameters(System.ReadOnlySpan<byte> pattern, System.ReadOnlySpan<byte> path)
		{
			this.pattern = pattern;
			this.path = path;
		}

		/// <summary>
		/// Get the value of the requested parameter.
		/// </summary>
		/// <param name="key">The key of the parameter to find.</param>
		/// <returns>The value of the parameter, or <see langword="default"/> when not found.</returns>
		/// <example><c>var post = routeParams.Get("post"u8);</c></example>
		public System.ReadOnlySpan<byte> Get(scoped System.ReadOnlySpan<byte> key) =>
			this.TryGet(key, out var result) ? result : default;

		/// <summary>
		/// Try to get the value of the requested parameter.
		/// </summary>
		/// <param name="key">The key of the parameter to find.</param>
		/// <param name="value">The value of the parameter, or <see langword="default"/> when not found.</param>
		/// <returns><see langword="true"/> when the value has been found, <see langword="false"/> otherwise.</returns>
		/// <example><c>var found = routeParams.TryGet("post"u8, out var post);</c></example>
		public bool TryGet(scoped System.ReadOnlySpan<byte> key, out System.ReadOnlySpan<byte> value)
		{
			// @todo Use a lookup table? (store an index per dynamic parameter?)
			var slash = RouteParameters.FindIndex(this.pattern, key);

			if (slash == -1)
			{
				value = default;
				return false;
			}

			var enumerator = new SplitEnumerator(this.path, Characters.PathDelimiter);

			// Keep moving the enumerator until we've arrived at the found slash index
			for (var i = 0; i <= slash; i++)
			{
				enumerator.MoveNext();
			}

			value = enumerator.Current;
			return true;
		}

		/// <summary>
		/// Get the value of the requested parameter and parse it to <typeparamref name="T"/>.
		/// </summary>
		/// <param name="key">The key of the parameter to find.</param>
		/// <returns>The value of the parameter, or <see langword="default"/> when not found or if unable to parse.</returns>
		/// <example><c>var post = routeParams.Get("post"u8);</c></example>
		public T? Get<T>(scoped System.ReadOnlySpan<byte> key) where T : System.IUtf8SpanParsable<T> =>
			this.TryGet<T>(key, out var result) ? result : default;

		/// <summary>
		/// Try to get the value of the requested parameter and parse it to <typeparamref name="T"/>.
		/// </summary>
		/// <param name="key">The key of the parameter to find.</param>
		/// <param name="value">The value of the parameter, or <see langword="default"/> when not found or if unable to parse.</param>
		/// <returns><see langword="true"/> when the value has been found and parsed, <see langword="false"/> otherwise.</returns>
		/// <example><c>var found = routeParams.TryGet("post"u8, out var post);</c></example>
		public bool TryGet<T>(scoped System.ReadOnlySpan<byte> key, [NotNullWhen(true)] out T? value)
			where T : System.IUtf8SpanParsable<T>
		{
			if (!this.TryGet(key, out var param))
			{
				value = default;
				return false;
			}

			return T.TryParse(param, CultureInfo.InvariantCulture, out value);
		}

		// Finds the index of the delimiter (start) before the value.
		private static int FindIndex(scoped System.ReadOnlySpan<byte> pattern, scoped System.ReadOnlySpan<byte> key)
		{
			// / + { + key + }
			System.Span<byte> search = stackalloc byte[1 + 1 + key.Length + 1];

			RouteParameters.FormatSearch(search, key);

			var idx = System.MemoryExtensions.IndexOf(pattern, search);

			if (idx == -1)
			{
				return -1;
			}

			// Get the entire pattern until the search value
			var slice = pattern.SliceUnsafe(0, idx + search.Length);

			// Count the amount of slashes (the delimiter). Now we have the index of the enumerated value that we need to return
			return System.MemoryExtensions.Count(slice, Characters.PathDelimiter);
		}

		// Formats the given key to follow the following format: /{[KEY]}
		private static void FormatSearch(scoped System.Span<byte> search, scoped System.ReadOnlySpan<byte> key)
		{
			search[0] = Characters.PathDelimiter;
			search[1] = Characters.RouteParameterStart;

			var copied = key.TryCopyTo(search.Slice(2));
			Debug.Assert(copied);

			search[^1] = Characters.RouteParameterEnd;
		}
	}
}
