using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Byrone.Xenia
{
	[PublicAPI]
	public static class RequestExtensions
	{
		/// <summary>
		/// Try to parse the UTF-8 encoded <see cref="Request.Body"/> representing a single JSON value into a <typeparamref name="TValue"/>.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="out"><see langword="null"/> if the parsing failed, otherwise a TValue representation of the JSON value.</param>
		/// <returns>A TValue representation of the JSON value.</returns>
		/// <returns><see langword="true"/> if the content was successfully parsed, <see langword="false"/> otherwise.</returns>
		/// <remarks>This function asserts that the request contains actual JSON data. You might want to manually check the <c>Content-Type</c> header.</remarks>
		public static bool TryGetBodyAsJson<TValue>(this in Request @this, [NotNullWhen(true)] out TValue? @out)
			where TValue : IJson<TValue>
		{
			// @todo Handle `Transfer-Encoding: chunked`

			try
			{
				@out = JsonSerializer.Deserialize<TValue>(@this.Body);
				return @out is not null;
			}
			catch (JsonException)
			{
				@out = default;
				return false;
			}
		}

		/// <summary>
		/// Parses the UTF-8 encoded <see cref="Request.Body"/> representing a single JSON value into a <typeparamref name="TValue"/>.
		/// </summary>
		/// <param name="this"></param>
		/// <typeparam name="TValue">The type to deserialize the JSON value into.</typeparam>
		/// <returns>A TValue representation of the JSON value.</returns>
		/// <exception cref="T:System.Text.Json.JsonException">The JSON is invalid or <typeparamref name="TValue" /> is not compatible with the JSON.</exception>
		/// <remarks>This function asserts that the request contains actual JSON data. You might want to manually check the <c>Content-Type</c> header.</remarks>
		public static TValue GetBodyAsJson<TValue>(this in Request @this) where TValue : IJson<TValue>
		{
			// @todo Handle `Transfer-Encoding: chunked`

			var result = JsonSerializer.Deserialize<TValue>(@this.Body);

			Debug.Assert(result is not null);

			return result;
		}
	}
}
