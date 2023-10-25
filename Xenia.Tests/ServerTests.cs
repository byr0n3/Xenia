using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;

namespace Xenia.Tests
{
	[TestClass]
	public sealed class ServerTests
	{
		internal static readonly int Port = System.Random.Shared.Next(100, 10000);

		private static readonly HttpClient httpClient = new()
		{
			BaseAddress = new System.Uri($"http://localhost:{ServerTests.Port}"),
		};

		private static CancellationTokenSource? tokenSource;
		private static Server? server;

		[AssemblyInitialize]
		public static void Setup(TestContext _)
		{
			ServerTests.tokenSource = new CancellationTokenSource();

			ServerTests.server = TestHelpers.CreateServer(ServerTests.tokenSource.Token);

			var thread = new Thread(ServerTests.server.Listen);
			thread.Start();
		}

		[AssemblyCleanup]
		public static void Cleanup()
		{
			ServerTests.tokenSource?.Cancel();

			ServerTests.server?.Dispose();
		}

		[TestMethod]
		public async Task ServerCanAddAndRemoveHandlersAsync()
		{
			if (ServerTests.server is null)
			{
				throw new System.Exception("Server not initialized");
			}

			var handler = new RequestHandler("/temp"u8, TempHandler);

			ServerTests.server.AddRequestHandler(in handler);

			var response = await ServerTests.httpClient.GetAsync("/temp").ConfigureAwait(false);

			Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);

			response.Dispose();

			ServerTests.server.RemoveRequestHandler(in handler);

			response = await ServerTests.httpClient.GetAsync("/temp").ConfigureAwait(false);

			Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);

			response.Dispose();

			return;

			static void TempHandler(in Request request, ref ResponseBuilder builder) =>
				builder.AppendHeaders(in StatusCodes.Status200OK,
									  request.HtmlVersion,
									  System.ReadOnlySpan<byte>.Empty,
									  "test/plain"u8,
									  0);
		}

		[TestMethod]
		public async Task ServerCanReturnHtmlAsync()
		{
			var response = await ServerTests.httpClient.GetAsync("/test").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "text/html");

			var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			Assert.IsTrue(string.Equals(html, TestHelpers.TestHtml, System.StringComparison.Ordinal));

			response.Dispose();
		}

		[TestMethod]
		public async Task ServerCanReturnRawJsonAsync()
		{
			var response = await ServerTests.httpClient.GetAsync("/json").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "application/json");

			var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			Assert.IsTrue(string.Equals(html, TestHelpers.TestJson, System.StringComparison.Ordinal));

			response.Dispose();
		}

		[TestMethod]
		public async Task ServerCanReturnJsonFromEntityAsync()
		{
			var response = await ServerTests.httpClient.GetAsync("/json/model").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "application/json");

			var content = await response.Content.ReadFromJsonAsync<Person>().ConfigureAwait(false);

			Assert.IsTrue(content == TestHelpers.Person);

			response.Dispose();
		}

		[TestMethod]
		public async Task ServerCanRenderRazorPageAsync()
		{
			var response = await ServerTests.httpClient.GetAsync("/razor").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "text/html");

			var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			// @todo Stricter?
			Assert.IsTrue(html.Contains(TestHelpers.TestRazor, System.StringComparison.Ordinal));

			response.Dispose();
		}

		[TestMethod]
		public async Task ServerCanParseAndReturnJsonAsync()
		{
			var response = await
				ServerTests.httpClient
						   .PostAsJsonAsync("/post/json", TestHelpers.Person)
						   .ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "application/json");

			var content = await response.Content.ReadFromJsonAsync<Person>().ConfigureAwait(false);

			Assert.IsTrue(content == TestHelpers.Person);

			response.Dispose();
		}

		[TestMethod]
		public async Task ServerCanReturn404NotFoundAsync()
		{
			try
			{
				var response = await ServerTests.httpClient.GetAsync("/").ConfigureAwait(false);

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
				var response = await ServerTests.httpClient.GetAsync("/post").ConfigureAwait(false);

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
