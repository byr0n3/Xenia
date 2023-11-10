using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;

namespace Xenia.Tests
{
	[TestClass]
	[SuppressMessage("Usage", "MA0040")]
	public sealed partial class ServerTests
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
			Assert.IsNotNull(ServerTests.server);

			var handler = new RequestHandler("/temp"u8, TempHandler);

			ServerTests.server.AddRequestHandler(in handler);

			var response = await ServerTests.httpClient.GetAsync("/temp").ConfigureAwait(false);

			Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);

			response.Dispose();

			Assert.IsTrue(ServerTests.server.RemoveRequestHandler(in handler));

			response = await ServerTests.httpClient.GetAsync("/temp").ConfigureAwait(false);

			Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);

			response.Dispose();

			return;

			static void TempHandler(in Request request, ref ResponseBuilder builder) =>
				builder.AppendHeaders(in request, in StatusCodes.Status200OK, default);
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

		[TestMethod]
		public async Task ServerCanResizeResponseBufferAsync()
		{
			var response = await ServerTests.httpClient.GetAsync("/resize").ConfigureAwait(false);

			// no need to validate the response body if the status code and content type are correct
			// server would've crashed before it could write the response if the response was too big

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "application/json");

			response.Dispose();
		}

		[TestMethod]
		public async Task ServerCanServeStaticFilesAsync()
		{
			var response = await ServerTests.httpClient.GetAsync("/_static/style.css").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "text/css");

			response.Dispose();

			response = await ServerTests.httpClient.GetAsync("/_static/js/main.js").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "application/javascript");

			response.Dispose();

			response = await ServerTests.httpClient.GetAsync("/file.txt").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "text/plain");

			response.Dispose();

			response = await ServerTests.httpClient.GetAsync("/nested/nested_file.txt").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "text/plain");

			response.Dispose();
		}
	}
}
