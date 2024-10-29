using System.Text.Json.Serialization;
using Byrone.Xenia.Example.Models;

namespace Byrone.Xenia.Example
{
	[JsonSourceGenerationOptions(
		GenerationMode = JsonSourceGenerationMode.Metadata,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
		PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower
	)]
	[JsonSerializable(typeof(Person))]
	internal sealed partial class AppJsonSerializerContext : JsonSerializerContext;
}
