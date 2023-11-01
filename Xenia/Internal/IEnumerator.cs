namespace Byrone.Xenia.Internal
{
	internal interface IEnumerator<out T> where T : struct
	{
		public T Current { get; }

		public bool MoveNext();
	}
}
