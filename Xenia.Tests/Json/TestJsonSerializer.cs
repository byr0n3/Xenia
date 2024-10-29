using System.Text.Json.Serialization;

namespace Byrone.Xenia.Tests.Json
{
	[JsonSourceGenerationOptions(
		GenerationMode = JsonSourceGenerationMode.Metadata,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
		PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower
	)]
	[JsonSerializable(typeof(Person))]
	internal sealed partial class TestJsonSerializer : JsonSerializerContext;
}
