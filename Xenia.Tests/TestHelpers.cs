using System.Linq;
using System.Net;
using System.Net.Http;

namespace Xenia.Tests
{
	internal static class TestHelpers
	{
		public static void AssertResponse(HttpResponseMessage? response, HttpStatusCode code, string expectedContentType)
		{
			Assert.IsNotNull(response);

			Assert.IsTrue(response.StatusCode == code);

			Assert.IsTrue(response.Content.Headers.TryGetValues("Content-Type", out var contentType));

			Assert.IsTrue(contentType.Any((value) => string.Equals(value, expectedContentType, System.StringComparison.Ordinal)));
		}
	}
}
