using System.Net;
using System.Threading.Tasks;

namespace Xenia.Tests
{
	public sealed partial class ServerTests
	{
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
		public async Task ServerCanRenderRazorPageAsync()
		{
			var response = await ServerTests.httpClient.GetAsync("/razor").ConfigureAwait(false);

			TestHelpers.AssertResponse(response, HttpStatusCode.OK, "text/html");

			var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			// @todo Stricter?
			Assert.IsTrue(html.Contains(TestHelpers.TestRazor, System.StringComparison.Ordinal));

			response.Dispose();
		}
	}
}
