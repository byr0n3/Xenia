using System.Diagnostics.CodeAnalysis;
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
		}

		[TestMethod]
		public async Task ServerCanParseQueryParametersAsync()
		{
			const string name = "John Doe";
			const int age = 21;

			var response = await ServerTests.httpClient.GetAsync($"/query?name={name}&age={age}").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "application/json");

			var person = await response.Content.ReadFromJsonAsync(Person.TypeInfo).ConfigureAwait(false);

			Assert.IsTrue(string.Equals(person.Name, name, System.StringComparison.Ordinal));
			Assert.IsTrue(person.Age == age);
		}

		[TestMethod]
		public async Task ServerCanServeStaticFilesAsync()
		{
			var response = await ServerTests.httpClient.GetAsync("/style.css").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "text/css");

			response = await ServerTests.httpClient.GetAsync("/js/main.js").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "application/javascript");

			response = await ServerTests.httpClient.GetAsync("/file.txt").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "text/plain");
		}
	}
}
