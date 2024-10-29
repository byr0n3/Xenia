using System.Text.Json.Serialization.Metadata;

namespace Byrone.Xenia
{
	public interface IJson<T> where T : IJson<T>
	{
		public static abstract JsonTypeInfo<T> TypeInfo { get; }
	}
}
