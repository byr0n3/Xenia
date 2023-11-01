namespace Byrone.Xenia.Internal
{
	internal interface IEnumerable<out TEnum, T>
		where TEnum : struct, IEnumerator<T>
		where T : struct
	{
		public TEnum GetEnumerator();
	}
}
