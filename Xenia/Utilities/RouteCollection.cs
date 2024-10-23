using System.Runtime.CompilerServices;
using Byrone.Xenia.Internal;
using Byrone.Xenia.Internal.Extensions;
using JetBrains.Annotations;

namespace Byrone.Xenia.Utilities
{
	/// <summary>
	/// Helper struct for finding the correct route/pattern while respecting route parameters.
	/// </summary>
	[PublicAPI]
	public readonly ref struct RouteCollection
	{
		private const byte delimiter = (byte)'/';
		private const byte parameterStart = (byte)'{';
		private const byte parameterEnd = (byte)'}';

		private readonly System.ReadOnlySpan<Unmanaged> routes;

		/// <summary>
		/// Create a new <see cref="RouteCollection"/> instance.
		/// </summary>
		/// <param name="routes">The routes to be able to compare against.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RouteCollection(System.ReadOnlySpan<Unmanaged> routes) =>
			this.routes = routes;

		/// <summary>
		/// Try to find the matching route pattern, based on the specified <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The request path to compare with.</param>
		/// <param name="result">The matching pattern when found, <see langword="default"/> otherwise.</param>
		/// <returns><see langword="true"/> when a matching pattern has been found, <see langword="false"/> otherwise.</returns>
		public bool TryFind(scoped System.ReadOnlySpan<byte> path, out System.ReadOnlySpan<byte> result)
		{
			// @todo Faster way than looping?
			foreach (var pattern in this.routes)
			{
				if (RouteCollection.Matches(pattern, path))
				{
					result = pattern;
					return true;
				}
			}

			result = default;
			return false;
		}

		// Checks if the pattern and path have the same amount of parts (delimiter count) and check if each part is equal/a route parameter.
		private static bool Matches(scoped System.ReadOnlySpan<byte> pattern, scoped System.ReadOnlySpan<byte> path)
		{
			var patternDelimiterCount = RouteCollection.DelimiterCount(pattern);
			var pathDelimiterCount = RouteCollection.DelimiterCount(path);

			if (patternDelimiterCount != pathDelimiterCount)
			{
				return false;
			}

			// Trim leading delimiters (/)
			RouteCollection.Trim(ref pattern);
			RouteCollection.Trim(ref path);

			var patternEnumerator = new SplitEnumerator(pattern, RouteCollection.delimiter);
			var pathEnumerator = new SplitEnumerator(path, RouteCollection.delimiter);

			for (var i = 0; i < patternDelimiterCount; i++)
			{
				patternEnumerator.MoveNext();
				pathEnumerator.MoveNext();

				if (!RouteCollection.MatchSlice(patternEnumerator.Current, pathEnumerator.Current))
				{
					return false;
				}
			}

			return true;
		}

		// Compare the current slice of the pattern and route
		private static bool MatchSlice(scoped System.ReadOnlySpan<byte> pattern, scoped System.ReadOnlySpan<byte> path)
		{
			// Current part is a route parameter
			if (!pattern.IsEmpty &&
				(pattern[0] == RouteCollection.parameterStart) && (pattern[^1] == RouteCollection.parameterEnd))
			{
				// @todo Test parameter value validity?
				return true;
			}

			return RouteCollection.Equals(path, pattern);
		}

		// Trim leading slashes
		private static void Trim(scoped ref System.ReadOnlySpan<byte> value)
		{
			if (value[0] == RouteCollection.delimiter)
			{
				value = value.SliceUnsafe(1);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int DelimiterCount(scoped System.ReadOnlySpan<byte> value) =>
			System.MemoryExtensions.Count(value, RouteCollection.delimiter);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool Equals(scoped System.ReadOnlySpan<byte> a, scoped System.ReadOnlySpan<byte> b) =>
			System.MemoryExtensions.SequenceEqual(a, b);
	}
}
