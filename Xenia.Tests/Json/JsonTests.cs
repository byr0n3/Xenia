using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Byrone.Xenia.Tests.Json
{
	public sealed class JsonTests
	{
		private const string jsonStr = "{\"first_name\":\"John\",\"last_name\":\"Doe\",\"dob\":\"2000-10-21\"}";

		private static readonly Person person = new()
		{
			FirstName = "John",
			LastName = "Doe",
			DateOfBirth = new System.DateOnly(2000, 10, 21),
		};

		[Fact]
		public void CanSerializeJson()
		{
			using var stream = new MemoryStream(128);
			using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
			{
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			});

			JsonSerializer.Serialize(writer, JsonTests.person);

			var json = System.MemoryExtensions.AsSpan(stream.GetBuffer(), 0, (int)stream.Position);
			var str = System.Text.Encoding.UTF8.GetString(json);

			Assert.Equal(JsonTests.jsonStr, str);
		}

		[Fact]
		public void CanDeserializeJson()
		{
			var value = JsonSerializer.Deserialize<Person>(JsonTests.jsonStr);

			Assert.Equal(JsonTests.person, value);
		}
	}
}
