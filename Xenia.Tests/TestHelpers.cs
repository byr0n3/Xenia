using System.Linq;
using System.Net;
using System.Net.Http;

namespace Xenia.Tests
{
	internal static class TestHelpers
	{
		public static void AssertHtml(HttpResponseMessage? response, HttpStatusCode code = HttpStatusCode.OK)
		{
			Assert.IsNotNull(response);

			Assert.IsTrue(response.StatusCode == code);

			Assert.IsTrue(response.Content.Headers.TryGetValues("Content-Type", out var contentType));

			Assert.IsTrue(contentType.Any(static (value) => string.Equals(value, "text/html", System.StringComparison.Ordinal)));
		}
	}
}
