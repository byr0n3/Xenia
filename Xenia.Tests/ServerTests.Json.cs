using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;

namespace Xenia.Tests
{
	public sealed partial class ServerTests
	{
		private static readonly Encoding enc = Encoding.UTF8;

		[TestMethod]
		public async Task ServerCanReturnRawJsonAsync()
		{
			Assert.IsNotNull(ServerTests.server);

			var handler = new RequestHandler("/json"u8, Handler);

			ServerTests.server.AddRequestHandler(in handler);

			var response = await ServerTests.httpClient.GetAsync("/json").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "application/json");

			var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			Assert.IsTrue(string.Equals(html, TestHelpers.TestJson, System.StringComparison.Ordinal));

			response.Dispose();

			Assert.IsTrue(ServerTests.server.RemoveRequestHandler(in handler));

			return;

			static void Handler(in Request request, ref ResponseBuilder response)
			{
				var bytes = ServerTests.enc.GetBytes(TestHelpers.TestJson);

				response.AppendJson(in request, in StatusCodes.Status200OK, bytes);
			}
		}

		[TestMethod]
		public async Task ServerCanReturnJsonFromEntityAsync()
		{
			Assert.IsNotNull(ServerTests.server);

			var handler = new RequestHandler("/json/model"u8, Handler);

			ServerTests.server.AddRequestHandler(in handler);

			var response = await ServerTests.httpClient.GetAsync("/json/model").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "application/json");

			var content = await response.Content.ReadFromJsonAsync(Person.TypeInfo).ConfigureAwait(false);

			Assert.IsTrue(content == TestHelpers.Person);

			response.Dispose();

			Assert.IsTrue(ServerTests.server.RemoveRequestHandler(in handler));

			return;

			static void Handler(in Request request, ref ResponseBuilder response) =>
				response.AppendJson(in request, in StatusCodes.Status200OK, TestHelpers.Person);
		}
	}
}
