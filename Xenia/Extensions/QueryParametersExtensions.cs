using Byrone.Xenia.Data;
using Byrone.Xenia.Helpers;
using JetBrains.Annotations;

namespace Byrone.Xenia.Extensions
{
	[PublicAPI]
	public static class QueryParametersExtensions
	{
		public static bool TryGetValue(this RentedArray<KeyValue> @this,
									   System.ReadOnlySpan<byte> key,
									   out KeyValue value)
		{
			foreach (var param in @this)
			{
				if (param.Key.Equals(key))
				{
					value = param;
					return true;
				}
			}

			value = default;
			return false;
		}
	}
}
