using System.Runtime.CompilerServices;

namespace Byrone.Xenia.Extensions
{
	internal static class SpanExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static System.ReadOnlySpan<byte> Slice(this System.ReadOnlySpan<byte> value, System.Range range) =>
			value.Slice(range.Start.Value, range.End.Value);

		public static int Split(this System.Span<byte> bytes, System.Span<System.Range> output, byte separator)
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

				output[count++] = new System.Range(lastIndex, i - lastIndex);

				lastIndex = i + 1;

				if (count >= output.Length)
				{
					break;
				}
			}

			if (count < output.Length)
			{
				output[count++] = new System.Range(lastIndex, size - lastIndex);
			}

			return count;
		}
	}
}
