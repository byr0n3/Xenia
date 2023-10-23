using System.Text.Json;
using Byrone.Xenia.Data;
using JetBrains.Annotations;

namespace Byrone.Xenia.Helpers
{
	[PublicAPI]
	public static class Json
	{
		public static bool TryGetBody<T>(in Request request, out T @out) where T : IJson<T>
		{
			if (!request.TryGetHeader("Content-Type"u8, out var contentHeader) ||
				!System.MemoryExtensions.SequenceEqual(contentHeader.Value, "application/json"u8))
			{
				@out = default;
				return false;
			}

			return Json.TryParse(request.Body, out @out);
		}

		public static bool TryParse<T>(System.ReadOnlySpan<byte> data, out T @out) where T : IJson<T>
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

		// @todo Return RentedArray<byte>
		public static byte[] Serialize<T>(T data) where T : IJson<T> =>
			JsonSerializer.SerializeToUtf8Bytes(data, T.TypeInfo);
	}
}
