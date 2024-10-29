using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using JetBrains.Annotations;
using Impl = System.Text.Json.JsonSerializer;

namespace Byrone.Xenia
{
	[PublicAPI]
	public static class JsonSerializer
	{
		/// <summary>
		/// Parses the text representing a single JSON value into a <typeparamref name="TValue"/>.
		/// </summary>
		/// <param name="utf8Json">JSON text to parse.</param>
		/// <typeparam name="TValue">The type to deserialize the JSON value into.</typeparam>
		/// <returns>A <typeparamref name="TValue" /> representation of the JSON value.</returns>
		/// <exception cref="T:System.Text.Json.JsonException">The JSON is invalid, <typeparamref name="TValue" /> is not compatible with the JSON, or there is remaining data in the string beyond a single JSON value.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TValue? Deserialize<TValue>(string utf8Json) where TValue : IJson<TValue> =>
			Impl.Deserialize(utf8Json, TValue.TypeInfo);

		/// <summary>
		/// Parses the UTF-8 encoded text representing a single JSON value into a <typeparamref name="TValue"/>.
		/// </summary>
		/// <param name="utf8Json">JSON text to parse.</param>
		/// <typeparam name="TValue">The type to deserialize the JSON value into.</typeparam>
		/// <returns>A <typeparamref name="TValue" /> representation of the JSON value.</returns>
		/// <exception cref="T:System.Text.Json.JsonException">The JSON is invalid, <typeparamref name="TValue" /> is not compatible with the JSON, or there is remaining data in the Stream.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TValue? Deserialize<TValue>(scoped System.ReadOnlySpan<byte> utf8Json)
			where TValue : IJson<TValue> =>
			Impl.Deserialize(utf8Json, TValue.TypeInfo);

		/// <summary>
		/// Parses the UTF-8 encoded text representing a single JSON value into a <typeparamref name="TValue"/>. The Stream will be read to completion.
		/// </summary>
		/// <param name="utf8Json">JSON text to parse.</param>
		/// <typeparam name="TValue">The type to deserialize the JSON value into.</typeparam>
		/// <returns>A <typeparamref name="TValue" /> representation of the JSON value.</returns>
		/// <exception cref="T:System.Text.Json.JsonException">The JSON is invalid, <typeparamref name="TValue" /> is not compatible with the JSON, or there is remaining data in the Stream.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TValue? Deserialize<TValue>(Stream utf8Json)
			where TValue : IJson<TValue> =>
			Impl.Deserialize(utf8Json, TValue.TypeInfo);

		/// <summary>Converts the provided value to UTF-8 encoded JSON text and write it to the <see cref="System.IO.Stream" />.</summary>
		/// <param name="utf8Json">The UTF-8 <see cref="System.IO.Stream" /> to write to.</param>
		/// <param name="value">The value to convert.</param>
		/// <typeparam name="TValue">The type of the value to serialize.</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize<TValue>(Stream utf8Json, TValue value) where TValue : IJson<TValue> =>
			Impl.Serialize(utf8Json, value, TValue.TypeInfo);

		/// <summary>Writes one JSON value (including objects or arrays) to the provided writer.</summary>
		/// <param name="utf8Json">The writer to write.</param>
		/// <param name="value">The value to convert.</param>
		/// <typeparam name="TValue">The type of the value to serialize.</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize<TValue>(Utf8JsonWriter utf8Json, TValue value) where TValue : IJson<TValue> =>
			Impl.Serialize(utf8Json, value, TValue.TypeInfo);
	}
}
