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

			if (request.TryGetHeader(Headers.TransferEncoding, out var encodingHeader) &&
				System.MemoryExtensions.StartsWith(encodingHeader.Value, "chunked"u8))
			{
				var length = ChunkedContent.ParseChunkedContent(request.Body, out var content);

				using (content)
				{
					if (length != 0)
					{
						return MultipartFormData.TryParse(content.AsSpan(0, length), out @out, out count);
					}
				}
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

			var count = bytes.Split(ranges.Data, Characters.NewLine);

			if (count == 0)
			{
				ranges.Dispose();

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
				if ((range.End.Value <= 2) && ((i + 1) < count))
				{
					contentBytes = bytes.SliceTrimmed(ranges[i + 1]);
					continue;
				}

				var line = bytes.SliceTrimmed(range);

				// separator, ignore
				if (System.MemoryExtensions.StartsWith(line, "--"u8))
				{
					continue;
				}

				var nameIdx = System.MemoryExtensions.IndexOf(line, "name="u8);

				// this line contains the name of the item
				if (nameIdx != -1)
				{
					nameIdx += 5;

					var nameLength = System.MemoryExtensions.IndexOf(line.Slice(nameIdx), (byte)'\\');

					nameBytes = line.Slice(nameIdx, nameLength == -1 ? line.Length - nameIdx : nameLength);

					continue;
				}

				if (nameBytes.IsEmpty || contentBytes.IsEmpty)
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

			ranges.Dispose();

			return true;
		}
	}
}
