using System.Text.Json;
using Byrone.Xenia.Data;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Internal;
using JetBrains.Annotations;

namespace Byrone.Xenia.Helpers
{
	[PublicAPI]
	public static class Json
	{
		public static bool TryParse<T>(in Request request, out T @out) where T : IJson<T>
		{
			// `StartsWith` in case the header contains the charset
			if (!request.TryGetHeader(Headers.ContentType, out var contentHeader) ||
				!System.MemoryExtensions.StartsWith(contentHeader.Value.AsSpan(), ContentTypes.Json))
			{
				@out = default;
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
						return Json.TryParse(content.AsSpan(0, length), out @out);
					}
				}
			}

			return Json.TryParse(request.Body, out @out);
		}

		private static bool TryParse<T>(System.ReadOnlySpan<byte> data, out T @out) where T : IJson<T>
		{
			try
			{
				var result = JsonSerializer.Deserialize(data, T.TypeInfo);

				if (result is null)
				{
					@out = default;
					return false;
				}

				@out = result;
				return true;
			}
			catch (JsonException)
			{
				@out = default;
				return false;
			}
		}

		// @todo Return RentedArray<byte>
		public static byte[] Serialize<T>(T data) where T : IJson<T> =>
			JsonSerializer.SerializeToUtf8Bytes(data, T.TypeInfo);
	}
}
