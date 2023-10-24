using Byrone.Xenia.Data;
using JetBrains.Annotations;

namespace Byrone.Xenia.Extensions
{
	[PublicAPI]
	public static class RequestExtensions
	{
		public static bool TryGetHeader(in this Request @this, System.ReadOnlySpan<byte> key, out RequestHeader header)
		{
			foreach (var head in @this.Headers)
			{
				if (!System.MemoryExtensions.SequenceEqual(head.Key, key))
				{
					continue;
				}

				header = head;
				return true;
			}

			header = default;
			return false;
		}
	}
}
