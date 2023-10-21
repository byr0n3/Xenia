using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;
using HttpMethod = Byrone.Xenia.Data.HttpMethod;

namespace Xenia.Tests
{
	[TestClass]
	public sealed class ServerTests
	{
		private const string testHtml = "<html><body><h1>Hello world!</h1></body></html>";
		internal const string RazorHtml = "This is a razor page!";

		private static readonly int port = System.Random.Shared.Next(100, 10000);
		private static readonly string baseUrl = $"http://localhost:{ServerTests.port}";

		private static CancellationTokenSource? tokenSource;
		private static Server? server;

		[AssemblyInitialize]
		public static void Setup(TestContext _)
		{
			ServerTests.tokenSource = new CancellationTokenSource();

			var options = new ServerOptions
			{
				IpAddress = "0.0.0.0",
				Port = ServerTests.port,
			};

			ServerTests.server = new Server(options, ServerTests.tokenSource.Token);

			ServerTests.server.AddRequestHandler(new RequestHandler("/test"u8, ServerTests.Handler));
			ServerTests.server.AddRequestHandler(new RequestHandler(HttpMethod.Post, "/post"u8, ServerTests.Handler));
			ServerTests.server.AddRazorPage<TestPage>("/razor"u8);

			var thread = new Thread(ServerTests.server.Listen);
			thread.Start();
		}

		[AssemblyCleanup]
		public static void Cleanup()
		{
			ServerTests.tokenSource?.Cancel();

			ServerTests.server?.Dispose();
		}

		private static void Handler(in Request request, ref ResponseBuilder response) =>
			response.AppendHtml(in request, in StatusCodes.Status200OK, ServerTests.testHtml);

		[TestMethod]
		public async Task ServerCanAddAndRemoveHandlersAsync()
		{
			if (ServerTests.server is null)
			{
				throw new System.Exception("Server not initialized");
			}

			var httpClient = new HttpClient();

			var handler = new RequestHandler("/temp"u8, TempHandler);

			ServerTests.server.AddRequestHandler(in handler);

			var response = await httpClient.GetAsync($"{ServerTests.baseUrl}/temp").ConfigureAwait(false);

			Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);

			response.Dispose();

			ServerTests.server.RemoveRequestHandler(handler);

			response = await httpClient.GetAsync($"{ServerTests.baseUrl}/temp").ConfigureAwait(false);

			Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);

			response.Dispose();

			return;

			static void TempHandler(in Request _, ref ResponseBuilder builder) =>
				builder.AppendHeaders(in StatusCodes.Status200OK, System.ReadOnlySpan<byte>.Empty, "test/plain"u8, 0);
		}

		[TestMethod]
		public async Task ServerCanReturnHtmlAsync()
		{
			var httpClient = new HttpClient();

			var response = await httpClient.GetAsync($"{ServerTests.baseUrl}/test").ConfigureAwait(false);

			TestHelpers.AssertHtml(response);

			var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			Assert.IsTrue(string.Equals(html, ServerTests.testHtml, System.StringComparison.Ordinal));

			response.Dispose();
		}

		[TestMethod]
		public async Task ServerCanRenderRazorPageAsync()
		{
			var httpClient = new HttpClient();

			var response = await httpClient.GetAsync($"{ServerTests.baseUrl}/razor").ConfigureAwait(false);

			TestHelpers.AssertHtml(response);

			var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			// @todo Stricter?
			Assert.IsTrue(html.Contains(ServerTests.RazorHtml, System.StringComparison.Ordinal));

			response.Dispose();
		}

		[TestMethod]
		public async Task ServerCanReturn404NotFoundAsync()
		{
			try
			{
				var httpClient = new HttpClient();

				var response = await httpClient.GetAsync(ServerTests.baseUrl).ConfigureAwait(false);

				Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);

				response.Dispose();
			}
			catch (HttpRequestException ex)
			{
				Assert.IsTrue(ex.StatusCode == HttpStatusCode.NotFound);
			}
		}

		[TestMethod]
		public async Task ServerCanReturn405MethodNotAllowedAsync()
		{
			try
			{
				var httpClient = new HttpClient();

				var response = await httpClient.GetAsync($"{ServerTests.baseUrl}/post").ConfigureAwait(false);

				Assert.IsTrue(response.StatusCode == HttpStatusCode.MethodNotAllowed);

				response.Dispose();
			}
			catch (HttpRequestException ex)
			{
				Assert.IsTrue(ex.StatusCode == HttpStatusCode.MethodNotAllowed);
			}
		}
	}
}
