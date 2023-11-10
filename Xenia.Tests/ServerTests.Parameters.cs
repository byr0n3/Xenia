using System.Threading.Tasks;
using System.Web;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;

namespace Xenia.Tests
{
	public sealed partial class ServerTests
	{
		[TestMethod]
		public async Task ServerCanParseRouteParametersAsync()
		{
			const string slug = "test-blog-post";

			Assert.IsNotNull(ServerTests.server);

			var handler = new RequestHandler("/posts/{post}"u8, Handler);

			ServerTests.server.AddRequestHandler(in handler);

			var response = await ServerTests.httpClient.GetAsync("/posts/" + slug).ConfigureAwait(false);

			response.Dispose();

			Assert.IsTrue(ServerTests.server.RemoveRequestHandler(in handler));

			return;

			static void Handler(in Request request, ref ResponseBuilder builder)
			{
				Assert.IsTrue(request.TryGetRouteParameter("post"u8, out var parameter));

				Assert.IsTrue(string.Equals(parameter.Value.ToString(), slug, System.StringComparison.Ordinal));

				builder.AppendHeaders(in request, in StatusCodes.Status200OK, ContentTypes.Json);
			}
		}

		[TestMethod]
		public async Task ServerCanParseQueryParametersAsync()
		{
			const string name = "John Doe";
			const int age = 21;

			Assert.IsNotNull(ServerTests.server);

			var handler = new RequestHandler("/query"u8, Handler);

			ServerTests.server.AddRequestHandler(in handler);

			var response = await ServerTests.httpClient.GetAsync($"/query?name={name}&age={age}").ConfigureAwait(false);

			response.Dispose();

			Assert.IsTrue(ServerTests.server.RemoveRequestHandler(in handler));

			return;

			static void Handler(in Request request, ref ResponseBuilder response)
			{
				var @params = QueryParameters.Parse(request.Query);

				Assert.IsTrue(@params.TryGetValue("name"u8, out var rawName));
				Assert.IsTrue(@params.TryGetValue("age"u8, out var rawAge));

				var requestName = HttpUtility.UrlDecode(rawName.Value.ToString() ?? string.Empty);
				var requestAge = int.Parse(rawAge.Value);

				Assert.IsTrue(string.Equals(requestName, name, System.StringComparison.Ordinal));
				Assert.IsTrue(requestAge == age);

				response.AppendHeaders(in request, in StatusCodes.Status200OK, default);

				@params.Dispose();
			}
		}
	}
}
