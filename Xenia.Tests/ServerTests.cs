using System.Net;
using System.Net.Http;
using System.Text;
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
		private const string testJson = "{\"test\": \"It seems to work!\"}";
		internal const string RazorHtml = "This is a razor page!";

		private static readonly TestModel testModel = new() { Value = "This is a test model." };

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
			ServerTests.server.AddRequestHandler(new RequestHandler("/json"u8, ServerTests.JsonHandler));
			ServerTests.server.AddRequestHandler(new RequestHandler("/json/model"u8, ServerTests.JsonModelHandler));
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

		private static void JsonHandler(in Request request, ref ResponseBuilder response) =>
			response.AppendJson(in request, in StatusCodes.Status200OK, Encoding.UTF8.GetBytes(ServerTests.testJson));

		private static void JsonModelHandler(in Request request, ref ResponseBuilder response) =>
			response.AppendJson(in request, in StatusCodes.Status200OK, ServerTests.testModel);

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
			var httpClient = new HttpClient();

			var response = await httpClient.GetAsync($"{ServerTests.baseUrl}/test").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "text/html");

			var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			Assert.IsTrue(string.Equals(html, ServerTests.testHtml, System.StringComparison.Ordinal));

			response.Dispose();
		}

		[TestMethod]
		public async Task ServerCanReturnRawJsonAsync()
		{
			var httpClient = new HttpClient();

			var response = await httpClient.GetAsync($"{ServerTests.baseUrl}/json").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "application/json");

			var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			Assert.IsTrue(string.Equals(html, ServerTests.testJson, System.StringComparison.Ordinal));

			response.Dispose();
		}

		[TestMethod]
		public async Task ServerCanReturnJsonFromEntityAsync()
		{
			var httpClient = new HttpClient();

			var response = await httpClient.GetAsync($"{ServerTests.baseUrl}/json/model").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "application/json");

			var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			Assert.IsTrue(string.Equals(html,
										Encoding.UTF8.GetString(Json.Serialize(ServerTests.testModel)),
										System.StringComparison.Ordinal));

			response.Dispose();
		}

		[TestMethod]
		public async Task ServerCanRenderRazorPageAsync()
		{
			var httpClient = new HttpClient();

			var response = await httpClient.GetAsync($"{ServerTests.baseUrl}/razor").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "text/html");

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
