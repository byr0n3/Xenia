using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Byrone.Xenia.Internal.Extensions;
using JetBrains.Annotations;

namespace Byrone.Xenia.Utilities
{
	/// <summary>
	/// Helper struct for retrieving and parsing query parameters from a URL/path.
	/// </summary>
	[PublicAPI]
	public readonly ref struct QueryParameters
	{
		private const byte queryDelimiter = (byte)'?';

		private readonly System.ReadOnlySpan<byte> query;

		/// <summary>
		/// Create a new <see cref="QueryParameters"/> instance.
		/// </summary>
		/// <param name="query">The query string to parse.</param>
		/// <remarks>The <paramref name="query"/> parameter is expected to start with '?'.</remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public QueryParameters(System.ReadOnlySpan<byte> query)
		{
			Debug.Assert(query[0] == QueryParameters.queryDelimiter);

			this.query = query;
		}

		/// <summary>
		/// Check if the query string contains the specified <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The parameter key to check for.</param>
		/// <returns><see langword="true"/> if the query string contains the <paramref name="key"/>, <see langword="false"/> otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(scoped System.ReadOnlySpan<byte> key) =>
			!this.Find(key).IsEmpty;

		/// <summary>
		/// Get the value of the specified <paramref name="key"/> from the query string.
		/// </summary>
		/// <param name="key">The parameter key to find.</param>
		/// <returns>The found parameter value, or <see langword="default"/> when not found.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public System.ReadOnlySpan<byte> Get(scoped System.ReadOnlySpan<byte> key) =>
			this.Find(key);

		/// <summary>
		/// Try to get the value of the specified <paramref name="key"/> from the query string.
		/// </summary>
		/// <param name="key">The parameter key to find.</param>
		/// <param name="value">The found value of the parameter, or <see langword="default"/> when not found.</param>
		/// <returns><see langword="true"/> if the parameter's value has been found, <see langword="false"/> otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGet(scoped System.ReadOnlySpan<byte> key, out System.ReadOnlySpan<byte> value)
		{
			value = this.Get(key);
			return !value.IsEmpty;
		}

		/// <summary>
		/// Get the value of the specified <paramref name="key"/> from the query string and parse it to <typeparamref name="T"/>.
		/// </summary>
		/// <param name="key">The parameter key to find.</param>
		/// <returns>The parsed value of the parameter, or <see langword="default"/> when not found or if unable to parse.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get<T>(scoped System.ReadOnlySpan<byte> key) where T : System.IUtf8SpanParsable<T>
		{
			var slice = this.Find(key);
			return T.Parse(slice, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Try to get the value of the specified <paramref name="key"/> from the query string and parse it to <typeparamref name="T"/>.
		/// </summary>
		/// <param name="key">The parameter key to find.</param>
		/// <param name="value">The parsed value of the parameter, or <see langword="default"/> when not found or if unable to parse.</param>
		/// <returns><see langword="true"/> if the parameter's value has been found and parsed, <see langword="false"/> otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGet<T>(scoped System.ReadOnlySpan<byte> key, [NotNullWhen(true)] out T? value)
			where T : System.IUtf8SpanParsable<T>
		{
			var slice = this.Find(key);

			if (slice.IsEmpty)
			{
				value = default;
				return false;
			}

			return T.TryParse(slice, CultureInfo.InvariantCulture, out value);
		}

		private System.ReadOnlySpan<byte> Find(scoped System.ReadOnlySpan<byte> key)
		{
			const byte delimiter = (byte)'=';

			if (this.query.IsEmpty)
			{
				return default;
			}

			var offset = key.Length;
			var idx = System.MemoryExtensions.IndexOf(this.query, key);

			if ((idx == -1) || (this.query[idx + offset] != delimiter))
			{
				return default;
			}

			var slice = this.query.SliceUnsafe(idx + offset + 1);

			var end = System.MemoryExtensions.IndexOf(slice, (byte)'&');

			return end == -1 ? slice : slice.SliceUnsafe(0, end);
		}

		/// <summary>
		/// Create a <see cref="QueryParameters"/> instance from a UTF-8 URL.
		/// </summary>
		/// <param name="pattern">The URL to retrieve the query parameters from.</param>
		/// <returns>The created <see cref="QueryParameters"/> instance when query parameters were found, <see langword="default"/> otherwise.</returns>
		/// <remarks>The given <paramref name="pattern"/> doesn't have to be a full URL, it can also be a relative path.</remarks>
		/// <example><c>QueryParameters.FromUrl("https://google.com?hello=world"u8);</c></example>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static QueryParameters FromUrl(System.ReadOnlySpan<byte> pattern)
		{
			var idx = System.MemoryExtensions.IndexOf(pattern, QueryParameters.queryDelimiter);

			return idx == -1 ? default : new QueryParameters(pattern.SliceUnsafe(idx));
		}
	}
}
