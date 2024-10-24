using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Byrone.Xenia.Utilities;

namespace Byrone.Xenia.Tests.Caching
{
	public sealed class CachingTests : BaseServerTests
	{
		private const long secondsInHour = 60 * 60;

		private static readonly System.DateTime lastModified = new(2024, 01, 01, 12, 0, 0);

		public CachingTests() : base(6002)
		{
		}

		[Fact]
		public async Task CanSendBasicCacheHeaders()
		{
			using (var request = new HttpRequestMessage(HttpMethod.Get, "/"))
			using (var response = await this.HttpClient.SendAsync(request))
			{
				Assert.True(response.IsSuccessStatusCode);

				Assert.NotNull(response.Headers.CacheControl);

				var age = response.Headers.CacheControl.MaxAge.GetValueOrDefault();

				Assert.True(response.Headers.CacheControl.Private);
				Assert.Equal(CachingTests.secondsInHour, age.TotalSeconds);
				Assert.Equal("\"default\"", response.Headers.ETag?.Tag);
			}
		}

		[Fact]
		public async Task CanSendNoCache()
		{
			using (var request = new HttpRequestMessage(HttpMethod.Get, "/no-cache"))
			using (var response = await this.HttpClient.SendAsync(request))
			{
				Assert.True(response.IsSuccessStatusCode);

				Assert.NotNull(response.Headers.CacheControl);

				var age = response.Headers.CacheControl.MaxAge.GetValueOrDefault();

				Assert.True(response.Headers.CacheControl.NoStore);
				Assert.True(response.Headers.CacheControl.NoCache);
				Assert.Equal(0, age.TotalSeconds);
				Assert.Null(response.Headers.ETag);
			}
		}

		[Fact]
		public async Task CanSendCacheWithVary()
		{
			using (var request = new HttpRequestMessage(HttpMethod.Get, "/with-vary"))
			using (var response = await this.HttpClient.SendAsync(request))
			{
				Assert.True(response.IsSuccessStatusCode);

				Assert.NotNull(response.Headers.CacheControl);

				var age = response.Headers.CacheControl.MaxAge.GetValueOrDefault();

				Assert.True(response.Headers.CacheControl.Public);
				Assert.Equal(CachingTests.secondsInHour, age.TotalSeconds);
				Assert.Equal("\"with-vary\"", response.Headers.ETag?.Tag);

				Assert.Equal("Accept-Language", response.Headers.Vary.ToString());
			}
		}

		[Fact]
		public async Task CanSendCacheWithLastModified()
		{
			using (var request = new HttpRequestMessage(HttpMethod.Get, "/with-last-modified"))
			using (var response = await this.HttpClient.SendAsync(request))
			{
				Assert.True(response.IsSuccessStatusCode);

				Assert.NotNull(response.Headers.CacheControl);

				var age = response.Headers.CacheControl.MaxAge.GetValueOrDefault();

				Assert.True(response.Headers.CacheControl.Public);
				Assert.Equal(CachingTests.secondsInHour, age.TotalSeconds);
				Assert.Equal("\"with-last-modified\"", response.Headers.ETag?.Tag);

				var receivedLastModified = response.Content.Headers.LastModified.GetValueOrDefault();

				Assert.Equal(new System.DateTimeOffset(CachingTests.lastModified), receivedLastModified);
			}
		}

		protected override IResponse RequestHandler(in Request request) =>
			new Response();

		private readonly struct Response : IResponse
		{
			public void Send(Socket client, in Request request)
			{
				System.Span<byte> cacheHeaders = stackalloc byte[256];

				var cacheable = Response.GetCacheable(in request);

				var headers = Cache.GetCacheHeaders(cacheHeaders, cacheable);

				var response = StringBuilder.Format(
					stackalloc byte[512],
					$"""
					 HTTP/1.1 200 OK
					 {headers}
					 Server: xenia-test-server


					 """
				);

				client.Send(response);
			}

			private static Cacheable GetCacheable(in Request request)
			{
				if (System.MemoryExtensions.StartsWith(request.Path, "/no-cache"u8))
				{
					return Cacheable.NoCache;
				}

				if (System.MemoryExtensions.StartsWith(request.Path, "/with-vary"u8))
				{
					return new Cacheable(
						CacheType.Public,
						(long)System.TimeSpan.FromHours(1).TotalSeconds,
						"with-vary"u8,
						"Accept-Language"u8
					);
				}

				if (System.MemoryExtensions.StartsWith(request.Path, "/with-last-modified"u8))
				{
					return new Cacheable(
						CacheType.Public,
						(long)System.TimeSpan.FromHours(1).TotalSeconds,
						"with-last-modified"u8,
						default,
						lastModified
					);
				}

				return new Cacheable(CacheType.Private, (long)System.TimeSpan.FromHours(1).TotalSeconds, "default"u8);
			}
		}
	}
}
