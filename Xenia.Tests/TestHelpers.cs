using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;
using HttpMethod = Byrone.Xenia.Data.HttpMethod;

namespace Xenia.Tests
{
	internal static class TestHelpers
	{
		public const string TestHtml = "<html><body><h1>Hello world!</h1></body></html>";
		public const string TestJson = "{\"test\": \"It seems to work!\"}";
		public const string TestRazor = "This is a razor page!";

		public static readonly Person Person = new()
		{
			Name = "John Doe",
			Age = 21,
		};

		public static Server CreateServer(CancellationToken token)
		{
			var options = new ServerOptions
			{
				IpAddress = "0.0.0.0",
				Port = ServerTests.Port,
			};

			var server = new Server(options, token);

			server.AddRequestHandler(new RequestHandler("/test"u8, Handler));
			server.AddRequestHandler(new RequestHandler("/json"u8, JsonHandler));
			server.AddRequestHandler(new RequestHandler("/json/model"u8, JsonModelHandler));
			server.AddRequestHandler(new RequestHandler(HttpMethod.Post, "/post"u8, Handler));
			server.AddRequestHandler(new RequestHandler(HttpMethod.Post, "/post/json"u8, PostJsonHandler));
			server.AddRazorPage<TestPage>("/razor"u8);

			return server;

			static void Handler(in Request request, ref ResponseBuilder response) =>
				response.AppendHtml(in request, in StatusCodes.Status200OK, TestHelpers.TestHtml);

			static void JsonHandler(in Request request, ref ResponseBuilder response) =>
				response.AppendJson(in request, in StatusCodes.Status200OK,
									Encoding.UTF8.GetBytes(TestHelpers.TestJson));

			static void JsonModelHandler(in Request request, ref ResponseBuilder response) =>
				response.AppendJson(in request, in StatusCodes.Status200OK, TestHelpers.Person);

			static void PostJsonHandler(in Request request, ref ResponseBuilder response)
			{
				if (!Json.TryParse(in request, out Person model))
				{
					response.AppendHeaders(in StatusCodes.Status500InternalServerError,
										   request.HtmlVersion,
										   System.ReadOnlySpan<byte>.Empty,
										   System.ReadOnlySpan<byte>.Empty,
										   0);
					return;
				}

				response.AppendJson(in request, in StatusCodes.Status200OK, model);
			}
		}

		public static void AssertResponse(HttpResponseMessage? response,
										  HttpStatusCode code,
										  string expectedContentType)
		{
			Assert.IsNotNull(response);

			Assert.IsTrue(response.StatusCode == code);

			Assert.IsTrue(response.Content.Headers.TryGetValues("Content-Type", out var contentType));

			Assert.IsTrue(
				contentType.Any((value) => string.Equals(value, expectedContentType, System.StringComparison.Ordinal)));
		}
	}
}
