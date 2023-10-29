using Byrone.Xenia.Data;
using Byrone.Xenia.Helpers;

namespace Byrone.Xenia.Extensions
{
	public static class MultipartFormDataExtensions
	{
		public static bool TryFindItem(this RentedArray<FormDataItem> @this,
									   int count,
									   System.ReadOnlySpan<byte> key,
									   out FormDataItem @out)
		{
			foreach (var item in @this.AsSpan(0, count))
			{
				if (System.MemoryExtensions.SequenceEqual(item.Name, key))
				{
					@out = item;
					return true;
				}
			}

			@out = default;
			return false;
		}
	}
}
