using System.Net;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;
using HttpMethod = Byrone.Xenia.Data.HttpMethod;

namespace Xenia.Tests;

[TestClass]
public sealed class BaseTests
{
	private const int port = 6969;
	private const string testHtml = "<html><body><h1>Hello world!</h1></body></html>";

	private static CancellationTokenSource? tokenSource;
	private static Server? server;

	[AssemblyInitialize]
	public static void Setup(TestContext _)
	{
		BaseTests.tokenSource = new CancellationTokenSource();

		var options = new ServerOptions
		{
			IpAddress = "0.0.0.0",
			Port = BaseTests.port,
		};

		BaseTests.server = new Server(options, BaseTests.tokenSource.Token);

		BaseTests.server.AddRequestHandler(new RequestHandler("/test"u8, BaseTests.SimpleHandler));
		BaseTests.server.AddRequestHandler(new RequestHandler(HttpMethod.Post, "/post"u8, BaseTests.SimpleHandler));

		var thread = new Thread(BaseTests.server.Listen);
		thread.Start();
	}

	[AssemblyCleanup]
	public static void Cleanup()
	{
		BaseTests.tokenSource?.Cancel();

		BaseTests.server?.Dispose();
	}

	private static void SimpleHandler(in Request request, ref ResponseBuilder response) =>
		response.AppendHtml(in request, in StatusCodes.Status200OK, BaseTests.testHtml);

	[TestMethod]
	public async Task ServerCanReturnHtml()
	{
		var httpClient = new HttpClient();

		var response = await httpClient.GetAsync($"http://localhost:{BaseTests.port}/test").ConfigureAwait(false);

		Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);

		Assert.IsTrue(response.Content.Headers.TryGetValues("Content-Type", out var contentType));

		Assert.IsTrue(contentType.Any(static (value) => value == "text/html"));

		var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

		Assert.IsTrue(html == BaseTests.testHtml);
	}

	[TestMethod]
	public async Task ServerCanReturn404NotFound()
	{
		try
		{
			var httpClient = new HttpClient();

			var response = await httpClient.GetAsync($"http://localhost:{BaseTests.port}").ConfigureAwait(false);

			Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
		}
		catch (HttpRequestException ex)
		{
			Assert.IsTrue(ex.StatusCode == HttpStatusCode.NotFound);
		}
	}

	[TestMethod]
	public async Task ServerCanReturn405MethodNotAllowed()
	{
		try
		{
			var httpClient = new HttpClient();

			var response = await httpClient.GetAsync($"http://localhost:{BaseTests.port}/post").ConfigureAwait(false);

			Assert.IsTrue(response.StatusCode == HttpStatusCode.MethodNotAllowed);
		}
		catch (HttpRequestException ex)
		{
			Assert.IsTrue(ex.StatusCode == HttpStatusCode.MethodNotAllowed);
		}
	}
}
