using System.Runtime.CompilerServices;

namespace Byrone.Xenia.Extensions
{
	internal static class SpanExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static System.ReadOnlySpan<byte> Slice(this System.ReadOnlySpan<byte> value, System.Range range) =>
			value.Slice(range.Start.Value, range.End.Value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Split(this System.Span<byte> bytes, System.Span<System.Range> ranges, byte separator) =>
			SpanExtensions.Split((System.ReadOnlySpan<byte>)bytes, ranges, separator);

		public static int Split(this System.ReadOnlySpan<byte> bytes, System.Span<System.Range> ranges, byte separator)
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
	}
}
