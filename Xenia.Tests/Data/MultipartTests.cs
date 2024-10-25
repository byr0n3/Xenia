using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Byrone.Xenia.Tests.Data
{
	// @todo Add more tests
	public sealed class MultipartTests : BaseServerTests
	{
		private static System.ReadOnlySpan<byte> FileContents =>
			"<html><body><p>This is a test HTML document, sent via multipart/form-data</p></body></html>"u8;

		public MultipartTests() : base(6004)
		{
		}

		[Fact]
		public async Task ServerCanParseMultipartData()
		{
			using (var request = new HttpRequestMessage(HttpMethod.Get, "/"))
			{
				var fileDataStream = new RentedMemoryStream(MultipartTests.FileContents.Length);
				fileDataStream.Write(MultipartTests.FileContents);

				var fileContent = new ByteArrayContent(fileDataStream.GetBuffer(), 0, (int)fileDataStream.Position);
				fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html");

				var ticks = System.DateTime.UtcNow.Ticks;
				var content = new MultipartFormDataContent(
					$"xenia-test-data-{ticks.ToString(NumberFormatInfo.InvariantInfo)}"
				);

				content.Add(new StringContent("admin"), "username");
				content.Add(fileContent, "file", "test-file.html");

				request.Content = content;

				using (var response = await this.HttpClient.SendAsync(request))
				{
					await fileDataStream.DisposeAsync();

					Assert.Equal(HttpStatusCode.OK, response.StatusCode);
				}
			}
		}

		protected override IResponse RequestHandler(in Request request) =>
			new Response();

		private readonly struct Response : IResponse
		{
			public void Send(Socket client, in Request request)
			{
				if (!request.TryGetHeader("Content-Type"u8, out var contentType))
				{
					client.Send("""
								HTTP/1.1 400 Bad Request
								Server: xenia-test-server


								"""u8);

					return;
				}

				var multipart = Multipart.FromContentType(contentType, request.Body);

				if (!multipart.IsValid)
				{
					client.Send("""
								HTTP/1.1 400 Bad Request
								Server: xenia-test-server


								"""u8);

					return;
				}

				Response.AssertValid(multipart, "username"u8, "text/plain"u8, "admin"u8);

				Response.AssertValid(multipart, "file"u8, "text/html"u8, MultipartTests.FileContents);

				client.Send("""
							HTTP/1.1 200 OK
							Server: xenia-test-server


							"""u8);
			}

			private static void AssertValid(Multipart multipart,
											scoped System.ReadOnlySpan<byte> name,
											scoped System.ReadOnlySpan<byte> expectedContentType,
											scoped System.ReadOnlySpan<byte> expectedValue)
			{
				var nameStr = System.Text.Encoding.UTF8.GetString(name);

				var found = multipart.TryGet(name, out var item);

				Assert.True(found, $"{nameStr}: not found");
				Assert.True(
					// Content-Type could contain things like charset
					System.MemoryExtensions.StartsWith(item.ContentType, expectedContentType),
					$"{nameStr}: Content-Type doesn't match"
				);
				Assert.Equal(expectedValue, item.Content);

				item.Dispose();
			}
		}
	}
}
