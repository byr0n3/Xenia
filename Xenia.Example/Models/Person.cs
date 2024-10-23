using System.Text.Json.Serialization.Metadata;

namespace Byrone.Xenia.Example.Models
{
	public readonly struct Person : IJson<Person>
	{
		public static JsonTypeInfo<Person> TypeInfo =>
			AppJsonSerializerContext.Default.Person!;

		public required string FirstName { get; init; }

		public required string LastName { get; init; }

		public required int Age { get; init; }
	}
}
