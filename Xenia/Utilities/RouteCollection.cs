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
			// If the path contains a query string, trim this off as it'll prevent matching routes.
			var queryIdx = System.MemoryExtensions.IndexOf(path, Characters.QueryParametersDelimiter);

			if (queryIdx != -1)
			{
				path = path.Slice(0, queryIdx);
			}

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
			var patternDelimiterCount = RouteCollection.PathDelimiterCount(pattern);
			var pathDelimiterCount = RouteCollection.PathDelimiterCount(path);

			if (patternDelimiterCount != pathDelimiterCount)
			{
				return false;
			}

			// Trim leading delimiters (/)
			RouteCollection.Trim(ref pattern);
			RouteCollection.Trim(ref path);

			var patternEnumerator = new SplitEnumerator(pattern, Characters.PathDelimiter);
			var pathEnumerator = new SplitEnumerator(path, Characters.PathDelimiter);

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
				(pattern[0] == Characters.RouteParameterStart) && (pattern[^1] == Characters.RouteParameterEnd))
			{
				// @todo Test parameter value validity?
				return true;
			}

			return RouteCollection.Equals(path, pattern);
		}

		// Trim leading slashes
		private static void Trim(scoped ref System.ReadOnlySpan<byte> value)
		{
			if (value[0] == Characters.PathDelimiter)
			{
				value = value.SliceUnsafe(1);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int PathDelimiterCount(scoped System.ReadOnlySpan<byte> value) =>
			System.MemoryExtensions.Count(value, Characters.PathDelimiter);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool Equals(scoped System.ReadOnlySpan<byte> a, scoped System.ReadOnlySpan<byte> b) =>
			System.MemoryExtensions.SequenceEqual(a, b);
	}
}
