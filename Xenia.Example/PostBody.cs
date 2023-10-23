using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Byrone.Xenia.Data;

namespace Byrone.Xenia.Example
{
	[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
								 DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonSerializable(typeof(PostBody))]
	internal sealed partial class PostBodySerializerContext : JsonSerializerContext;

	public readonly struct PostBody : IJson<PostBody>
	{
		public static JsonTypeInfo<PostBody> TypeInfo =>
			PostBodySerializerContext.Default.PostBody;

		[JsonPropertyName("name")] public string Name { get; init; }

		[JsonPropertyName("age")] public int Age { get; init; }
	}
}
