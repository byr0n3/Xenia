using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading.Tasks;
using Byrone.Xenia.Utilities;

namespace Byrone.Xenia.Tests.Encoding
{
	public sealed class EncodingTests : BaseServerTests
	{
		private static System.ReadOnlySpan<byte> Body =>
			"<html><body>Hello World</body></html>"u8;

		public EncodingTests() : base(6001)
		{
		}

		[Fact]
		public async Task CanRespectEncodingPreference()
		{
			using (var request = new HttpRequestMessage(HttpMethod.Get, "/"))
			{
				request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
				request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip", 1.0));
				request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("*", 0.5));

				using (var response = await this.HttpClient.SendAsync(request))
				{
					Assert.Equal("deflate", response.Content.Headers.ContentEncoding.First());
					Assert.Equal("xenia-test-server", response.Headers.Server.ToString());
					Assert.Equal("text/html", response.Content.Headers.ContentType?.ToString());
				}
			}

			using (var request = new HttpRequestMessage(HttpMethod.Get, "/"))
			{
				request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br", 0.9));
				request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip", 0.6));
				request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate", 0.5));

				using (var response = await this.HttpClient.SendAsync(request))
				{
					Assert.Equal("br", response.Content.Headers.ContentEncoding.First());
					Assert.Equal("xenia-test-server", response.Headers.Server.ToString());
					Assert.Equal("text/html", response.Content.Headers.ContentType?.ToString());
				}
			}
		}

		[Fact]
		public Task CanEncodeBodyUsingBrotli() =>
			this.AssertEncodingAsync("br");

		[Fact]
		public Task CanEncodeBodyUsingGzip() =>
			this.AssertEncodingAsync("gzip");

		[Fact]
		public Task CanEncodeBodyUsingDeflate() =>
			this.AssertEncodingAsync("deflate");

		private async Task AssertEncodingAsync(string encoding)
		{
			using (var request = new HttpRequestMessage(HttpMethod.Get, "/"))
			{
				request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(encoding));

				using (var response = await this.HttpClient.SendAsync(request).ConfigureAwait(false))
				{
					Assert.Equal(encoding, response.Content.Headers.ContentEncoding.First());
					Assert.Equal("xenia-test-server", response.Headers.Server.ToString());
					Assert.Equal("text/html", response.Content.Headers.ContentType?.ToString());

					var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

					var decoded = new byte[128];
					var read = 0;

					switch (encoding)
					{
						case "br":
							var brotli = new BrotliStream(content, CompressionMode.Decompress, true);
							read = brotli.Read(decoded);
							break;

						case "gzip":
							var gzip = new GZipStream(content, CompressionMode.Decompress, true);
							read = gzip.Read(decoded);
							break;

						case "deflate":
							var deflate = new DeflateStream(content, CompressionMode.Decompress, true);
							read = deflate.Read(decoded);
							break;
					}

					await content.DisposeAsync().ConfigureAwait(false);

					Assert.Equal(
						EncodingTests.Body,
						System.MemoryExtensions.AsSpan(decoded, 0, read)
					);
				}
			}
		}

		protected override IResponse RequestHandler(in Request request) =>
			new Response();

		private readonly struct Response : IResponse
		{
			public void Send(Socket client, in Request request)
			{
				var found = request.TryGetEncoding(out var encoding);

				Assert.True(found);
				Assert.False(encoding.IsEmpty);

				var stream = new RentedMemoryStream(64);

				var compressed = BodyEncoding.TryEncode(stream, encoding, EncodingTests.Body);

				Assert.True(compressed);

				var response = StringBuilder.Format(
					stackalloc byte[128],
					$"""
					 HTTP/1.1 200 OK
					 Content-Encoding: {encoding}
					 Content-Type: text/html
					 Server: xenia-test-server


					 """
				);

				client.Send(response);
				client.Send(System.MemoryExtensions.AsSpan(stream.GetBuffer(), 0, (int)stream.Position));

				stream.Dispose();
			}
		}
	}
}
