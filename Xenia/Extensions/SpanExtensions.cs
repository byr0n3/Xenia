using System.Runtime.CompilerServices;
using Byrone.Xenia.Helpers;
using Bytes = System.ReadOnlySpan<byte>;

namespace Byrone.Xenia.Extensions
{
	internal static class SpanExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Bytes Slice(this Bytes value, System.Range range) =>
			value.Slice(range.Start.Value, range.End.Value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Split(this System.Span<byte> bytes, System.Span<System.Range> ranges, byte separator) =>
			SpanExtensions.Split((Bytes)bytes, ranges, separator);

		public static int Split(this Bytes bytes, System.Span<System.Range> ranges, byte separator)
		{
			var size = bytes.Length;

			var count = 0;
			var lastIndex = 0;

			for (var i = 0; i < size; i++)
			{
				if (bytes[i] != separator)
				{
					continue;
				}

				ranges[count++] = new System.Range(lastIndex, i - lastIndex);

				lastIndex = i + 1;

				if (count >= ranges.Length)
				{
					break;
				}
			}

			if (count < ranges.Length)
			{
				ranges[count++] = new System.Range(lastIndex, size - lastIndex);
			}

			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Bytes SliceTrimmed(this Bytes @this, System.Range range) =>
			SpanExtensions.SliceTrimmed(@this, range.Start.Value, range.End.Value);

		public static Bytes SliceTrimmed(this Bytes @this, int start, int length)
		{
			if (@this[(start + length - 1)] == Characters.NewLine)
			{
				length--;
			}

			if (@this[(start + length - 1)] == Characters.Reset)
			{
				length--;
			}

			// safety measure
			if (length < 0)
			{
				// get the remainder of the data
				length = @this.Length - start;
			}

			return @this.Slice(start, length);
		}
	}
}
