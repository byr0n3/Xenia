using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Byrone.Xenia.Data;

namespace Byrone.Xenia.Example
{
	[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
								 DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonSerializable(typeof(Person))]
	internal sealed partial class PersonSerializerContext : JsonSerializerContext;

	public readonly struct Person : IJson<Person>
	{
		public static JsonTypeInfo<Person> TypeInfo =>
			PersonSerializerContext.Default.Person;

		[JsonPropertyName("name")] public string Name { get; init; }

		[JsonPropertyName("age")] public int Age { get; init; }
	}
}
