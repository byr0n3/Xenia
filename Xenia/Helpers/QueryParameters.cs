using Byrone.Xenia.Data;
using JetBrains.Annotations;
using Bytes = System.ReadOnlySpan<byte>;

namespace Byrone.Xenia.Helpers
{
	[PublicAPI]
	public static class QueryParameters
	{
		public static RentedArray<KeyValue> Parse(Bytes query)
		{
			if (query.IsEmpty)
			{
				return default;
			}

			var length = 1 + System.MemoryExtensions.Count(query, Characters.Ampersand);

			var result = new RentedArray<KeyValue>(length);

			var start = 0;
			var count = 0;

			for (var i = 0; i < length; i++)
			{
				var sliced = query.Slice(start);
				var idx = System.MemoryExtensions.IndexOf(sliced, Characters.Ampersand);

				if (idx > 0)
				{
					sliced = sliced.Slice(0, idx);
					start = idx + 1;
				}

				idx = System.MemoryExtensions.IndexOf(sliced, Characters.Equals);

				// query parameter has no value
				if (idx == -1)
				{
					result[count++] = new KeyValue(sliced, default);
					continue;
				}

				result[count++] = new KeyValue(sliced.Slice(0, idx), sliced.Slice(idx + 1));
			}

			return result;
		}
	}
}
