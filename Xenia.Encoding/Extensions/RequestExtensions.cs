using System.Globalization;
using Byrone.Xenia.Internal;
using Byrone.Xenia.Internal.Extensions;

// ReSharper disable once CheckNamespace
namespace Byrone.Xenia
{
	public static class RequestExtensions
	{
		/// <summary>
		/// Try to get the preferred encoding from the request's <c>Accept-Encoding</c> header.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="result">A preferred encoding when found, <see langword="default"/> otherwise.</param>
		/// <returns><see langword="true"/> when a supported encoding has been found, <see langword="false"/> otherwise.</returns>
		public static bool TryGetEncoding(this in Request @this, out System.ReadOnlySpan<byte> result)
		{
			var separator = ", "u8;

			if (!@this.TryGetHeader("Accept-Encoding"u8, out var encoding))
			{
				result = default;
				return default;
			}

			var values = System.MemoryExtensions.Count(encoding, separator) + 1;

			if (values == 1)
			{
				result = RequestExtensions.Parse(encoding, out _);
				return true;
			}

			var peak = 0f;
			result = default;

			foreach (var value in new SpanSplitEnumerator(encoding, separator))
			{
				var current = RequestExtensions.Parse(value, out var weight);

				if (result.IsEmpty || (weight > peak))
				{
					result = current;
					peak = weight;
				}
			}

			if (System.MemoryExtensions.SequenceEqual(result, "*"u8))
			{
				result = "gzip"u8; // @todo Configurable fallback?
			}

			return true;
		}

		/// <summary>
		/// Parse a part of the <c>Accept-Encoding</c> header.
		/// </summary>
		/// <param name="value">The part of the header to parse.</param>
		/// <param name="weight">The suggested weight of the encoding, <c>1.1f</c> otherwise (no weight = overrule rest).</param>
		/// <returns>The name of the encoding.</returns>
		private static System.ReadOnlySpan<byte> Parse(System.ReadOnlySpan<byte> value, out float weight)
		{
			var separator = "; q="u8;

			var weightIdx = System.MemoryExtensions.IndexOf(value, separator);

			if (weightIdx != -1)
			{
				float.TryParse(
					value.SliceUnsafe(weightIdx + separator.Length),
					NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
					NumberFormatInfo.InvariantInfo,
					out weight
				);
			}
			else
			{
				// If there's no weight assigned, give this a higher weight than the highest weight (1.0f)
				// No weight = highest preference
				weight = 1.1f;
			}

			var length = weightIdx == -1 ? value.Length : weightIdx;

			return value.SliceUnsafe(0, length);
		}
	}
}
