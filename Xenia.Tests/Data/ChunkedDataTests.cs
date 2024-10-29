using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Byrone.Xenia.Internal.Extensions;
using Byrone.Xenia.Utilities;

namespace Byrone.Xenia.Tests.Data
{
	public sealed class ChunkedDataTests : BaseServerTests
	{
		public ChunkedDataTests() : base(6003)
		{
		}

		[Fact]
		public async Task ServerCanParseChunkedData()
		{
			const string data = "Hello world! This is some chunked data.";

			using (var request = new HttpRequestMessage(HttpMethod.Post, "/"))
			{
				request.Content = new StringContent(data);
				request.Headers.TransferEncodingChunked = true;

				using (var response = await this.HttpClient.SendAsync(request))
				{
					Assert.True(response.IsSuccessStatusCode);

					Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);

					var content = await response.Content.ReadAsStringAsync();

					Assert.Equal(data, content);
				}
			}
		}

		[Fact]
		public async Task ServerCanParseEncodedChunkedData()
		{
			var memoryStream = new RentedMemoryStream(128);
			var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal, true);
			{
				gzipStream.Write("Hello world! This is some chunked data."u8);
				await gzipStream.DisposeAsync();
			}

			using (var request = new HttpRequestMessage(HttpMethod.Post, "/"))
			{
				request.Content = new ByteArrayContent(memoryStream.GetBuffer(), 0, (int)memoryStream.Position);
				request.Headers.TransferEncoding.ParseAdd("gzip, chunked");

				using (var response = await this.HttpClient.SendAsync(request))
				{
					await memoryStream.DisposeAsync();

					Assert.True(response.IsSuccessStatusCode);

					Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);

					var content = await response.Content.ReadAsStringAsync();

					Assert.Equal("Hello world! This is some chunked data.", content);
				}
			}
		}

		protected override IResponse RequestHandler(in Request request) =>
			new Response();

		private readonly struct Response : IResponse
		{
			public void Send(Socket client, in Request request)
			{
				if (!ChunkedData.HasChunkedBody(in request))
				{
					client.Send("HTTP/1.1 400 Bad Request\r\n"u8);

					return;
				}

				var size = ChunkedData.GetSize(request.Body);

				if (size <= 0)
				{
					client.Send("HTTP/1.1 400 Bad Request\r\n"u8);

					return;
				}

				System.Span<byte> buffer = stackalloc byte[size];

				var written = ChunkedData.Parse(request.Body, buffer);

				var body = buffer.SliceUnsafe(0, written);

				var encoding = ChunkedData.GetChunkEncoding(in request);

				if (encoding.IsEmpty)
				{
					var response = StringBuilder.Format(
						stackalloc byte[256],
						$"""
						 HTTP/1.1 200 OK
						 Content-Type: text/plain
						 Server: xenia-test-server

						 {body}
						 """
					);

					client.Send(response);
				}
				else
				{
					Response.SendDecoded(client, encoding, body);
				}
			}

			private static void SendDecoded(Socket client,
											scoped System.ReadOnlySpan<byte> encoding,
											scoped System.Span<byte> data)
			{
				System.Span<byte> dst = stackalloc byte[128];

				var decoded = BodyEncoding.TryDecode(dst, data, encoding, out var written);

				Debug.Assert(decoded);

				var body = dst.SliceUnsafe(0, written);

				var response = StringBuilder.Format(
					stackalloc byte[256],
					$"""
					 HTTP/1.1 200 OK
					 Content-Type: text/plain
					 Server: xenia-test-server

					 {body}
					 """
				);

				client.Send(response);
			}
		}
	}
}
