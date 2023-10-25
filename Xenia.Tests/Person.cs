using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Xenia.Tests
{
	[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
								 DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonSerializable(typeof(Person))]
	internal sealed partial class PersonSerializerContext : JsonSerializerContext;

	public readonly struct Person : IJson<Person>, System.IEquatable<Person>
	{
		public static JsonTypeInfo<Person> TypeInfo =>
			PersonSerializerContext.Default.Person;

		public string Name { get; init; }

		public int Age { get; init; }

		public bool Equals(Person other) =>
			string.Equals(this.Name, other.Name, System.StringComparison.Ordinal) &&
			this.Age == other.Age;

		public override bool Equals(object? @object) =>
			@object is Person other && this.Equals(other);

		public override int GetHashCode() =>
			System.HashCode.Combine(this.Name, this.Age);

		public static bool operator ==(Person left, Person right) =>
			left.Equals(right);

		public static bool operator !=(Person left, Person right) =>
			!left.Equals(right);
	}
}
