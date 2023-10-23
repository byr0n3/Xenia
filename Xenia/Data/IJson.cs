using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;

namespace Byrone.Xenia.Data
{
	public interface IJson<T> where T : IJson<T>
	{
		[SuppressMessage("Design", "CA1000")] public static abstract JsonTypeInfo<T> TypeInfo { get; }
	}
}
