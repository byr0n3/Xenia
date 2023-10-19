using JetBrains.Annotations;

namespace Byrone.Xenia.Extensions
{
	[PublicAPI]
	public static class RentedArrayExtensions
	{
		public static bool TryGetHeader(this RentedArray<RequestHeader> headers,
										System.ReadOnlySpan<byte> key,
										out RequestHeader header)
		{
			foreach (var head in headers.Data)
			{
				if (System.MemoryExtensions.SequenceEqual(head.Key, key))
				{
					header = head;
					return true;
				}
			}

			header = default;
			return false;
		}
	}
}
