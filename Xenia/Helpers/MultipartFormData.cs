using Byrone.Xenia.Data;
using Byrone.Xenia.Extensions;

namespace Byrone.Xenia.Helpers
{
	public static class MultipartFormData
	{
		public static bool TryParse(in Request request, out RentedArray<FormDataItem> @out, out int count)
		{
			// we use `StartsWith` here because the Content-Type header also contains the name of the form data regions
			if (!request.TryGetHeader(Headers.ContentType, out var contentHeader) ||
				!System.MemoryExtensions.StartsWith(contentHeader.Value.AsSpan, ContentTypes.MultipartFormData))
			{
				@out = default;
				count = 0;
				return false;
			}

			return MultipartFormData.TryParse(request.Body, out @out, out count);
		}

		// @todo File support
		// @todo REFACTOR
		private static bool TryParse(System.ReadOnlySpan<byte> bytes,
									 out RentedArray<FormDataItem> @out,
									 out int outCount)
		{
			// @todo Lower + resizable
			var ranges = new RentedArray<System.Range>(32);

			var count = bytes.Split(ranges.Data, (byte)'\n');

			if (count == 0)
			{
				@out = default;
				outCount = 0;
				return false;
			}

			@out = new RentedArray<FormDataItem>(count);
			outCount = 0;

			System.ReadOnlySpan<byte> nameBytes = default;
			System.ReadOnlySpan<byte> contentBytes = default;

			for (var i = 1; i < count; i++)
			{
				var range = ranges[i];

				// empty line, next range is item content
				if ((range.End.Value == 0) && ((i + 1) < count))
				{
					contentBytes = bytes.Slice(ranges[i + 1]);
					continue;
				}

				var line = ServerHelpers.Strip(bytes, range);

				var nameIdx = System.MemoryExtensions.IndexOf(line, "name=\""u8);

				// this line contains the name of the item
				if (nameIdx != -1)
				{
					nameIdx += 6;

					var nameLength = System.MemoryExtensions.IndexOf(line.Slice(nameIdx), "\""u8);

					nameBytes = line.Slice(nameIdx, nameLength);

					continue;
				}

				// separator, ignore
				if (!System.MemoryExtensions.StartsWith(line, "------"u8))
				{
					continue;
				}

				@out[outCount++] = new FormDataItem
				{
					Name = nameBytes,
					Content = contentBytes,
				};

				nameBytes = default;
				contentBytes = default;
			}

			return true;
		}
	}
}
