using System.Text;
using System.Threading;
using Byrone.Xenia.Data;
using Byrone.Xenia.Extensions;
using Byrone.Xenia.Helpers;

namespace Byrone.Xenia.Example
{
	internal static class Program
	{
		public static void Main(string[] args)
		{
			var cancelTokenSource = new CancellationTokenSource();

			var options = new ServerOptions
			{
				IpAddress = "0.0.0.0",
				Port = 6969,
				Logger = new ConsoleLogger(),
			};

			var server = new Server(options, cancelTokenSource.Token);

			server.AddRazorPage<Test>("/"u8);
			server.AddRazorPage<Test>("/test"u8);

			server.AddRequestHandler(new RequestHandler("/html"u8, Program.RawHtmlHandler));
			server.AddRequestHandler(new RequestHandler("/json"u8, Program.JsonHandler));
			server.AddRequestHandler(new RequestHandler(HttpMethod.Post, "/post"u8, Program.PostHandler));

			var thread = new Thread(server.Listen);

			thread.Start();
		}

		private static void RawHtmlHandler(in Request request, ref ResponseBuilder response)
		{
			var html = $"<html><body><h1>Hello from {Encoding.UTF8.GetString(request.Path)}!</h1></html></body>";

			response.AppendHtml(in request, in StatusCodes.Status200OK, html);
		}

		private static void JsonHandler(in Request request, ref ResponseBuilder response)
		{
			var json = "{\"test\": \"Hello there!\", \"number\": 69}";

			var jsonSize = Encoding.UTF8.GetByteCount(json);

			// @todo request.TryGetHeader("Accept-Encoding"u8, out var encodingHeader)
			response.AppendHeaders(in StatusCodes.Status200OK,
								   System.ReadOnlySpan<byte>.Empty,
								   "application/json"u8,
								   jsonSize);

			var span = response.Take(jsonSize);

			var written = Encoding.UTF8.GetBytes(json, span);

			response.Move(written);
		}

		private static void PostHandler(in Request request, ref ResponseBuilder builder)
		{
			if (!Json.TryGetBody(in request, out PostBody body))
			{
				builder.AppendHeaders(in StatusCodes.Status500InternalServerError,
									  System.ReadOnlySpan<byte>.Empty,
									  System.ReadOnlySpan<byte>.Empty,
									  0);
				return;
			}

			builder.AppendJson(in request, in StatusCodes.Status200OK, body);
		}
	}
}
