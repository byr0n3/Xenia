using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Xenia.Tests
{
	[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
								 DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonSerializable(typeof(TestModel))]
	internal sealed partial class TestModelSerializerContext : JsonSerializerContext;

	public readonly struct TestModel : IJson<TestModel>
	{
		public static JsonTypeInfo<TestModel> TypeInfo =>
			TestModelSerializerContext.Default.TestModel;

		public string Value { get; init; }
	}
}
