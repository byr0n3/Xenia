using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Byrone.Xenia
{
	/// <summary>
	/// Entity describing the cache behavior of certain content.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public readonly ref struct Cacheable
	{
		public static Cacheable NoCache =>
			new(CacheType.NoCache, default, default);

		/// <summary>
		/// The type of cache to use.
		/// </summary>
		public readonly CacheType Type;

		/// <summary>
		/// How long the browser can save this response before it needs revalidation.
		/// </summary>
		/// <remarks>This value is expected to be in <b>seconds</b>.</remarks>
		public readonly long Age;

		/// <summary>
		/// Identifier for a specific version of the resource. This could be a (file)version, but also a hash of the response body.
		/// </summary>
		/// <remarks>The HTTP specification expects this value to be quoted using the '"' character. This is <b>NOT</b> done automatically.</remarks>
		/// <example><c>"tag-here"</c>, <c>"v0.1.0"</c></example>
		public readonly System.ReadOnlySpan<byte> ETag;

		/// <summary>
		/// Which headers of the response can vary between cached responses.
		/// </summary>
		/// <remarks>
		/// <para>If the <c>Vary</c> header is set to <c>Accept-Language</c>, the browser will check if the request's <c>Accept-Language</c> is the same as the cached one.</para>
		/// <para>If it is, it will use the cached response (assuming it's not stale). Otherwise, it will execute the request and store the response alongside the cached response (which has a different header).</para>
		/// </remarks>
		/// <example><c>"Accept-Language"u8</c>, <c>"Accept-Language, Accept"u8</c></example>
		public readonly System.ReadOnlySpan<byte> Vary;

		/// <summary>
		/// The date and time when the server believes the resource was last modified.
		/// Conditional requests containing <c>If-Modified-Since</c> or <c>If-Unmodified-Since</c> headers make use of this field.
		/// </summary>
		/// <remarks>This header is less accurate than an <see cref="ETag"/> header, it is a fallback mechanism and using <see cref="ETag"/> is preferred.</remarks>
		public readonly System.DateTime LastModified;

		public Cacheable(CacheType type,
						 long age,
						 System.ReadOnlySpan<byte> etag,
						 System.ReadOnlySpan<byte> vary = default,
						 System.DateTime lastModified = default)
		{
			// The cacheable should have an ETag, unless the response is not allowed to be cached
			Debug.Assert((type == CacheType.NoCache) || !etag.IsEmpty);

			this.Type = type;
			this.Age = age;
			this.ETag = etag;
			this.Vary = vary;
			this.LastModified = lastModified;
		}
	}

	public enum CacheType
	{
		/// <summary>
		/// Browser is <b>NOT</b> allowed to cache the response.
		/// </summary>
		NoCache,

		/// <summary>
		/// The response can be stored in the machine-wide shared cache.
		/// </summary>
		/// <remarks>This is <b>NOT</b> recommended for authorized responses, for example those which require an authenticated user.</remarks>
		Public,

		/// <summary>
		/// The response can only be stored in the private/local cache of the machine's user.
		/// </summary>
		/// <remarks>This type should be used for content that is personalized for the user making the request.</remarks>
		Private,
	}
}
