using System.Globalization;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;

namespace Byrone.Xenia.Internal
{
	internal static class ChunkedContent
	{
		public static int ParseChunkedContent(System.ReadOnlySpan<byte> data, out RentedArray<byte> result)
		{
			System.Span<System.Range> ranges = stackalloc System.Range[32];

			var count = data.Split(ranges, Characters.NewLine);

			if (count == 0)
			{
				result = default;
				return 0;
			}

			result = new RentedArray<byte>(data.Length);
			var nextLength = 0;
			var offset = 0;

			for (var i = 0; i < count; i++)
			{
				var range = ranges[i];

				// empty line, we can skip
				if (range.End.Value <= 1)
				{
					continue;
				}

				var line = data.SliceTrimmed(range);

				if (nextLength == 0)
				{
					// length is declared in hex
					nextLength = int.Parse(line, NumberStyles.HexNumber);
					continue;
				}

				line.CopyTo(result.AsSpan(offset, nextLength));
				offset += nextLength;
				nextLength = 0;
			}

			return offset;
		}
	}
}
