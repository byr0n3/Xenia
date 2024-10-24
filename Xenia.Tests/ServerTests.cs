using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Byrone.Xenia.Internal;

namespace Byrone.Xenia.Tests
{
	public sealed class ServerTests : BaseServerTests
	{
		public ServerTests() : base(6000)
		{
		}

		[Fact]
		public async Task ServerCanHandleGetRequest()
		{
			using (var request = new HttpRequestMessage(HttpMethod.Get, "/"))
			using (var response = await this.HttpClient.SendAsync(request))
			{
				Assert.True(response.IsSuccessStatusCode);

				Assert.Equal("xenia-test-server", response.Headers.Server.ToString());
				Assert.Equal("text/html", response.Content.Headers.ContentType?.ToString());

				var content = await response.Content.ReadAsStringAsync();

				Assert.Equal("<html><body><h1>Hello world!</h1></body></html>", content);
			}
		}

		[Fact]
		public async Task ServerCanHandlePostRequest()
		{
			const string requestContent = "<h1>Hello world!</h1>";

			using (var request = new HttpRequestMessage(HttpMethod.Post, "/"))
			{
				request.Content = new StringContent(requestContent, System.Text.Encoding.UTF8, "text/html");

				using (var response = await this.HttpClient.SendAsync(request))
				{
					Assert.True(response.IsSuccessStatusCode);

					Assert.Equal("xenia-test-server", response.Headers.Server.ToString());
					Assert.Equal("text/html", response.Content.Headers.ContentType?.ToString());

					var content = await response.Content.ReadAsStringAsync();

					Assert.Equal(requestContent, content);
				}
			}
		}

		protected override IResponse RequestHandler(in Request request)
		{
			if (System.MemoryExtensions.SequenceEqual(request.Method, "POST"u8))
			{
				return new PostResponse(request.Body);
			}

			return new GetResponse();
		}

		private readonly struct GetResponse : IResponse
		{
			public void Send(Socket client, in Request _)
			{
				var response = """
							   HTTP/1.1 200 OK
							   Content-Type: text/html
							   Server: xenia-test-server

							   <html><body><h1>Hello world!</h1></body></html>
							   """u8;

				client.Send(response);
			}
		}

		private readonly struct PostResponse : IResponse, System.IDisposable
		{
			private readonly RentedArray<byte> body;

			public PostResponse(scoped System.ReadOnlySpan<byte> body)
			{
				this.body = new RentedArray<byte>(body.Length);

				var copied = body.TryCopyTo(this.body);

				Debug.Assert(copied);
			}

			public void Send(Socket client, in Request _)
			{
				var response = """
							   HTTP/1.1 200 OK
							   Content-Type: text/html
							   Server: xenia-test-server


							   """u8;

				client.Send(response);
				client.Send(this.body.Span);
			}

			public void Dispose() =>
				this.body.Dispose();
		}
	}
}
