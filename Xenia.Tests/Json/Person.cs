using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Byrone.Xenia.Tests.Json
{
	public readonly struct Person : IJson<Person>, System.IEquatable<Person>
	{
		public static JsonTypeInfo<Person> TypeInfo =>
			TestJsonSerializer.Default.Person!;

		public required string FirstName { get; init; }

		public required string LastName { get; init; }

		[JsonPropertyName("dob")] public required System.DateOnly DateOfBirth { get; init; }

		[JsonIgnore]
		public int Age =>
			(int)((System.DateTime.UtcNow - this.DateOfBirth.ToDateTime(default)).TotalDays / 365.242199);

		public bool Equals(Person other) =>
			string.Equals(this.FirstName, other.FirstName, System.StringComparison.Ordinal) &&
			string.Equals(this.LastName, other.LastName, System.StringComparison.Ordinal) &&
			this.DateOfBirth.Equals(other.DateOfBirth);

		public override bool Equals(object? obj) =>
			obj is Person other && this.Equals(other);

		public override int GetHashCode() =>
			System.HashCode.Combine(this.FirstName, this.LastName, this.DateOfBirth);

		public static bool operator ==(Person left, Person right) =>
			left.Equals(right);

		public static bool operator !=(Person left, Person right) =>
			!left.Equals(right);
	}
}
